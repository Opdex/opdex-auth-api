using System.Collections.Generic;
using Opdex.Auth.Api.Encryption;

namespace Opdex.Auth.Api.Models;

public class JsonWebKeySetResponseModel
{
    public JsonWebKeySetResponseModel(IEnumerable<RsaPubJsonWebKeyItem> keys)
    {
        Keys = keys;
    }
    
     public IEnumerable<RsaPubJsonWebKeyItem> Keys { get; }
}