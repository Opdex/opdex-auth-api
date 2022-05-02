using Opdex.Auth.Domain;

namespace Opdex.Auth.Api.Models;

public class AuthorizeRequestQuery
{
    public ResponseType ResponseType { get; set; }
    
    public string? RedirectUri { get; set; }
    
    public string? CodeChallenge { get; set; }

    public CodeChallengeMethod CodeChallengeMethod { get; set; } = CodeChallengeMethod.Plain;

    public string? State { get; set; }
}