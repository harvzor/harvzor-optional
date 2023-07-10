using System.Text.Json;
using System.Text.Json.Serialization;
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
    
    [Fact]
    public void WriteJson_ShouldIgnoreProperty_WhenPropertyIsIgnored()
    {
        // Arrange

        FooWithIgnoredProperty foo = new()
        {
            OptionalProperty = "some value"
        };

        // Act

        string json = Serialize(foo);

        // Assert

        json.ShouldBe("{}");
    }
    
    private class FooWithIgnoredProperty
    {
        [JsonIgnore]
        public Optional<string> OptionalProperty { get; set; }
    }
}
