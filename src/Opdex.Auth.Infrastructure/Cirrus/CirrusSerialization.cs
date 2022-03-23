using System.Text.Json;

namespace Opdex.Auth.Infrastructure.Cirrus;

internal static class CirrusSerialization
{
    public static readonly JsonSerializerOptions DefaultOptions;

    static CirrusSerialization()
    {
        DefaultOptions = new JsonSerializerOptions();
        DefaultOptions.Converters.Add(new BooleanAsStringConverter());
    }
}