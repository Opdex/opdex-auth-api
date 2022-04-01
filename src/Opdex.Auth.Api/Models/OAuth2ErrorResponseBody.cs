namespace Opdex.Auth.Api.Models;

public class OAuth2ErrorResponseBody
{
    public OAuth2ErrorResponseBody(string error, string? errorDescription = null, string? errorUri = null)
    {
        Error = error;
        ErrorDescription = errorDescription;
        ErrorUri = errorUri;
    }
    
    public string Error { get; }
    public string? ErrorDescription { get; }
    public string? ErrorUri { get; }
}