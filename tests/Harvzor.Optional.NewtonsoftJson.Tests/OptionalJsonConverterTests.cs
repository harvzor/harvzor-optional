using Harvzor.Optional.JsonConverter.BaseTests;
using Newtonsoft.Json;

namespace Harvzor.Optional.NewtonsoftJson.Tests;

public class OptionalJsonConverterTests : OptionalJsonConverterBaseTests
{
    protected override T Deserialize<T>(string str)
    {
        return JsonConvert.DeserializeObject<T>(str, new OptionalJsonConverter())!;
    }

    protected override string Serialize(object obj)
    {
        return JsonConvert.SerializeObject(obj, new OptionalJsonConverter())!;
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
