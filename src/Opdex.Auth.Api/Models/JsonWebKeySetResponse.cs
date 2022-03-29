using System.Collections.Generic;
using Opdex.Auth.Api.Encryption;

namespace Opdex.Auth.Api.Models;

public class JsonWebKeySetResponse
{
    public JsonWebKeySetResponse(IEnumerable<RsaPubJsonWebKeyItem> keys)
    {
        Keys = keys;
    }
    
     public IEnumerable<RsaPubJsonWebKeyItem> Keys { get; }
}