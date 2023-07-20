using System.Text.Json;
using System.Text.Json.Serialization;

namespace Harvzor.Optional.SystemTextJson.Tests;

public class VersionConverter : JsonConverter<Version>
{
    public override Version Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string s = reader.GetString()!;
        
        return new Version(s);
    }

    public override void Write(Utf8JsonWriter writer, Version value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
