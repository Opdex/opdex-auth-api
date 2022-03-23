using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace Opdex.Auth.Api.Conventions;

/// <summary>
/// Ignores telemetry from non-api paths
/// </summary>
public class IgnoreRequestPathsTelemetryProcessor : ITelemetryProcessor
{
    private readonly ITelemetryProcessor _next;

    public IgnoreRequestPathsTelemetryProcessor(ITelemetryProcessor next)
    {
        _next = next;
    }

    public void Process(ITelemetry item)
    {
        if (item is RequestTelemetry request &&
            (request.Url.AbsolutePath is "/" or "/favicon.ico" ||
             request.Url.AbsolutePath.StartsWith("/swagger"))) return;

        _next.Process(item);
    }
}