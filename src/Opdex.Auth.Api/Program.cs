using System;
using System.Reflection;
using System.Text.Json.Serialization;
using AspNetCoreRateLimit;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using FluentValidation;
using FluentValidation.AspNetCore;
using Hellang.Middleware.ProblemDetails;
using Hellang.Middleware.ProblemDetails.Mvc;
using MediatR;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using Opdex.Auth.Api;
using Opdex.Auth.Api.Conventions;
using Opdex.Auth.Api.Encryption;
using Opdex.Auth.Api.SignalR;
using Opdex.Auth.Domain;
using Opdex.Auth.Infrastructure;
using Serilog;
using JsonSerializer = System.Text.Json.JsonSerializer;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

if (builder.Environment.IsProduction())
{
    builder.WebHost.ConfigureAppConfiguration((context, config) =>
    {
        var secretClient = new SecretClient(
            new Uri($"https://{context.Configuration[$"{AzureKeyVaultOptions.ConfigurationSectionName}:Name"]}.vault.azure.net/"),
            new DefaultAzureCredential());

        config.AddAzureKeyVault(secretClient, new AzureKeyVaultConfigurationOptions
        {
            Manager = new KeyVaultSecretManager(),
            ReloadInterval = TimeSpan.FromSeconds(16)
        });
    });
}

builder.Services.AddApplicationInsightsTelemetry(new ApplicationInsightsServiceOptions
{
    // disable default adaptive sampling configuration, instead we customise this
    EnableAdaptiveSampling = false
});

// gets rid of telemetry spam in the debug console, may prevent visual studio app insights monitoring
TelemetryDebugWriter.IsTracingDisabled = true;

builder.Services.Configure<StatusOptions>(builder.Configuration);
builder.Services.Configure<ApiOptions>(builder.Configuration);
builder.Services.Configure<PromptOptions>(builder.Configuration);
builder.Services.Configure<EncryptionOptions>(builder.Configuration.GetSection(EncryptionOptions.ConfigurationSectionName));
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.ConfigurationSectionName));
builder.Services.Configure<AzureKeyVaultOptions>(builder.Configuration.GetSection(AzureKeyVaultOptions.ConfigurationSectionName));

builder.Services.AddMediatR(typeof(IDomainAssemblyMarker), typeof(IApiAssemblyMarker), typeof(IInfrastructureAssemblyMarker));
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddScoped<ITwoWayEncryptionProvider, AesCbcProvider>();
builder.Services.AddScoped<IJwtIssuer, JwtIssuer>();
builder.Services.AddScoped<StratisIdValidator>();
builder.Services.AddSignalR(o => { o.EnableDetailedErrors = true; }).AddAzureSignalR();
builder.Services.AddApiVersioning(options =>
{
    options.ReportApiVersions = true;
    options.ErrorResponses = new ProblemDetailsApiVersionErrorProvider();
    options.DefaultApiVersion = new ApiVersion(1, 0);
});

// Rate Limiting
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"))
    .Configure<IpRateLimitOptions>(options =>
    {
        options.RequestBlockedBehaviorAsync = async (httpContext, _, rateLimitCounter, rule) =>
        {
            var detail = $"Quota exceeded. Maximum allowed: {rule.Limit} per {rule.Period}.";
            var response = ProblemDetailsBuilder.PrepareResponse(httpContext, StatusCodes.Status429TooManyRequests, detail);
            httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            httpContext.Response.Headers["Retry-After"] = rateLimitCounter.Timestamp.RetryAfterFrom(rule);
            await httpContext.Response.WriteAsync(JsonSerializer.Serialize(response));
            await httpContext.Response.CompleteAsync();
        };
    });
builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

builder.Services.AddProblemDetails(options =>
{
    options.ValidationProblemStatusCode = StatusCodes.Status400BadRequest;
    options.MapToStatusCode<Exception>(StatusCodes.Status500InternalServerError);
    options.IncludeExceptionDetails = (_, _) => builder.Environment.IsDevelopment();
    options.ShouldLogUnhandledException = (httpContext, exception, _) =>
    {
        httpContext.Items.Add("UnhandledException", exception);
        return false;
    };
});

builder.Services.AddFluentValidation();
builder.Services.AddValidatorsFromAssembly(typeof(IApiAssemblyMarker).Assembly);
builder.Services.AddControllers(config => config.ValueProviderFactories.Add(new SnakeCaseValueProviderFactory()))
                .AddJsonOptions(config => config.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull);
builder.Services.AddProblemDetailsConventions();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    IdentityModelEventSource.ShowPII = true;
}
else
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseCors(options => options
                .SetIsOriginAllowed(host => true)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials());

var telemetryProcessorChainBuilder = app.Services.GetRequiredService<TelemetryConfiguration>().DefaultTelemetrySink.TelemetryProcessorChainBuilder;
telemetryProcessorChainBuilder.UseAdaptiveSampling(5, "Exception");
telemetryProcessorChainBuilder.Use(next => new IgnoreRequestPathsTelemetryProcessor(next));
telemetryProcessorChainBuilder.Build();

app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext += (diagnosticContext, httpContext) =>
    {
        if (httpContext.Items.TryGetValue("UnhandledException", out var exception))
        {
            diagnosticContext.SetException((Exception)exception!);
        }
    };
});

// yaml mapping not supported by default, must explicitly map
var fileExtensionContentTypeProvider = new FileExtensionContentTypeProvider();
fileExtensionContentTypeProvider.Mappings.Add(".yml", "text/yaml");
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = fileExtensionContentTypeProvider,
    FileProvider = new ManifestEmbeddedFileProvider(Assembly.GetExecutingAssembly()),
    RequestPath = "/swagger/v1"
});

app.UseSwaggerUI(options =>
{
    options.RoutePrefix = "swagger";
    options.InjectJavascript("v1/openapi.js");
});

app.UseIpRateLimiting();

app.MapHub<AuthHub>("/v1/socket");
app.MapControllers();

app.Run();
