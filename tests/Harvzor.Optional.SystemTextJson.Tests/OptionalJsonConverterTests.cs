using System.Text.Json;
using Harvzor.Optional.JsonConverter.BaseTests;

namespace Harvzor.Optional.SystemTextJson.Tests;

public class OptionalJsonConverterTests : OptionalJsonConverterBaseTests
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        Converters =
        {
            new OptionalJsonConverter()
        }
    };

    protected override T Deserialize<T>(string str)
    {
        return JsonSerializer.Deserialize<T>(str, _jsonSerializerOptions)!;
    }

    protected override string Serialize(object obj)
    {
        return JsonSerializer.Serialize(obj, _jsonSerializerOptions);
    }
}
