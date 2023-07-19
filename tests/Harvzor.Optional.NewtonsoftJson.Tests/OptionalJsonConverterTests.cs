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
    [Trait("Category","Custom Attribute")]
    public void WriteJson_ShouldIgnoreProperty_WhenPropertyIsIgnored()
    {
        // Arrange

        ClassWithIgnoredProperty classWithIgnoredProperty = new()
        {
            OptionalProperty = "some value"
        };

        // Act

        string json = Serialize(classWithIgnoredProperty);

        // Assert

        json.ShouldBe("{}");
    }
    
    private class ClassWithIgnoredProperty
    {
        [JsonIgnore]
        public Optional<string> OptionalProperty { get; set; }
    }
    
    [Fact]
    [Trait("Category","Custom Attribute")]
    public void WriteJson_ShouldCorrectlyNameProperty_WhenPropertyHasNameOverwritten()
    {
        // Arrange

        ClassWithPropertyWithOverwrittenName classWithPropertyWithOverwrittenName = new()
        {
            OptionalProperty = "some value"
        };

        // Act

        string json = Serialize(classWithPropertyWithOverwrittenName);

        // Assert

        json.ShouldBe("{\"OptProperty\":\"some value\"}");
    }
    
    private class ClassWithPropertyWithOverwrittenName
    {
        [JsonProperty("OptProperty")]
        public Optional<string> OptionalProperty { get; set; }
    }
}
