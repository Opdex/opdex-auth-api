namespace Opdex.Auth.Api.Encryption;

public abstract class JsonWebKeyItem
{
    protected JsonWebKeyItem(string kid, string kty, string? alg = null, string? use = null)
    {
        Kid = kid;
        Kty = kty;
        Alg = alg;
        Use = use;
    }

    public string? Alg { get; }

    public string Kid { get; }

    public string Kty { get;}
    
    public string? Use { get; }
}