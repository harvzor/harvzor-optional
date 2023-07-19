using Newtonsoft.Json;

namespace Harvzor.Optional.NewtonsoftJson.Tests;

/// <summary>
/// https://www.newtonsoft.com/json/help/html/CustomJsonConverterGeneric.htm
/// </summary>
public class VersionConverter : Newtonsoft.Json.JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        string s = (string)reader.Value;

        return new Version(s);
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(Version);
    }
}
