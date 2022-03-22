using Microsoft.AspNetCore.Http;

namespace Opdex.Auth.Api.Helpers;

public static class HttpRequestExtensions
{
    public static string BaseUrl(this HttpRequest httpRequest) => $"{httpRequest.Scheme}://{httpRequest.Host}";
    
    public static string BaseUrlWithPath(this HttpRequest httpRequest) => $"{BaseUrl(httpRequest)}{httpRequest.Path}";
}