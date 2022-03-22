using System;
using System.Reflection;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Opdex.Auth.Api;
using Opdex.Auth.Api.Conventions;
using Opdex.Auth.Api.Encryption;
using Opdex.Auth.Api.SignalR;
using Opdex.Auth.Domain;
using Opdex.Auth.Domain.Cirrus;
using Opdex.Auth.Infrastructure;
using Opdex.Auth.Infrastructure.Cirrus;
using Opdex.Auth.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsProduction())
{
    builder.WebHost.ConfigureAppConfiguration((context, config) =>
    {
        var secretClient = new SecretClient(
            new Uri($"https://{context.Configuration["Azure:KeyVault:Name"]}.vault.azure.net/"),
            new DefaultAzureCredential());

        config.AddAzureKeyVault(secretClient, new AzureKeyVaultConfigurationOptions
        {
            Manager = new KeyVaultSecretManager(),
            ReloadInterval = TimeSpan.FromSeconds(16)
        });
    });
}

builder.Services.Configure<StatusOptions>(builder.Configuration);
builder.Services.Configure<EncryptionOptions>(builder.Configuration.GetSection(EncryptionOptions.Name));

builder.Services.AddMediatR(typeof(IDomainAssemblyMarker), typeof(IApiAssemblyMarker), typeof(IInfrastructureAssemblyMarker));
builder.Services.Configure<DatabaseOptions>(builder.Configuration.GetSection(DatabaseOptions.Name));
builder.Services.AddTransient<IDbContext, DbContext>();
builder.Services.Configure<CirrusOptions>(builder.Configuration.GetSection(CirrusOptions.Name));
builder.Services.AddHttpClient<IWalletModule, WalletModule>();

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

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseHttpsRedirection();

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

app.UseAuthorization();

app.MapHub<AuthHub>("/v1/socket");
app.MapControllers();

app.Run();
