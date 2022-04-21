using System.Collections.Generic;
using Opdex.Auth.Api.Encryption;

namespace Opdex.Auth.Api.Models;

public class JsonWebKeySetResponseBody
{
    public JsonWebKeySetResponseBody(IEnumerable<RsaPubJsonWebKeyItem> keys)
    {
        Keys = keys;
    }
    
     public IEnumerable<RsaPubJsonWebKeyItem> Keys { get; }
}