using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.KeyVaultExtensions;

namespace Opdex.Auth.Api.Encryption;

/// <summary>
/// Key vault security key that is amended to customise the kid value present in the token header
/// </summary>
public class AmendedKeyVaultSecurityKey : KeyVaultSecurityKey
{
    public AmendedKeyVaultSecurityKey(string keyId, string kid, AuthenticationCallback authenticationCallback)
        : base(keyId, authenticationCallback)
    {
        Kid = kid;
    }
    
    public string Kid { get; }
    
    public override string KeyId
    {
        get
        {
            // as the JWT header is built from the keyId, we return a custom kid if the calling class is JwtHeader
            var frame = new StackFrame(1);
            var callingType = frame.GetMethod()?.DeclaringType;
            return callingType is not null && callingType == typeof(JwtHeader) ? Kid : base.KeyId;
        }
        set => base.KeyId = value;
    }
}