using System.Text.RegularExpressions;

namespace Opdex.Auth.Api.Validation;

public static class OAuth2Standards
{ 
    internal static readonly Regex CodeVerifierAndChallengeRegex = new("^[a-zA-Z0-9-._~]*$", RegexOptions.Compiled);
    
    /// <summary>
    /// Supports inputting padded base64url values for increased flexibility
    /// </summary>
    internal static readonly Regex Base64UrlEncodedCodeChallenge = new("^[a-zA-Z0-9-._~]*==$", RegexOptions.Compiled);
}