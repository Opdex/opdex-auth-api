using System.Text.Json;
using System.Text.Json.Serialization;

namespace Opdex.Auth.Infrastructure.Cirrus;

internal class BooleanAsStringConverter : JsonConverter<bool>
{
    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.True:
                return true;
            case JsonTokenType.False:
                return false;
            case JsonTokenType.String:
                if (bool.TryParse(reader.GetString(), out var result)) return result;
                break;
        }
        
        throw new JsonException("Boolean value could not be converted");
    }

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}