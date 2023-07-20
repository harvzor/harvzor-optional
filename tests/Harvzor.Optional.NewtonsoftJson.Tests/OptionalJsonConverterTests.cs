using Harvzor.Optional.JsonConverter.BaseTests;
using Newtonsoft.Json;

namespace Harvzor.Optional.NewtonsoftJson.Tests;

public class OptionalJsonConverterTests : OptionalJsonConverterBaseTests
{
    protected override T Deserialize<T>(string str)
    {
        return JsonConvert.DeserializeObject<T>(str, new JsonSerializerSettings()
        {
            // TODO: setting to indented doesn't change the output as the output has been overwritten...
            // Formatting = Formatting.Indented,
            Converters = new List<Newtonsoft.Json.JsonConverter>
            {
                new OptionalJsonConverter(),
            },
        })!;
    }

    protected override string Serialize(object obj)
    {
        return JsonConvert.SerializeObject(obj, new JsonSerializerSettings
        {
            Converters = new List<Newtonsoft.Json.JsonConverter>
            {
                new OptionalJsonConverter()
            },
            ContractResolver = new OptionalShouldSerializeContractResolver(),
        })!;
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
    
    [Fact]
    [Trait("Category","Works With Other Custom Converter")]
    public void WriteJson_ShouldWriteValue_WhenOptionalPropertyInObjectIsDefinedAndThereIsAnotherConverter()
    {
        // Arrange

        ClassWithOptionalPropertyAndVersionConverter classWithOptionalPropertyAndVersionConverter = new ClassWithOptionalPropertyAndVersionConverter
        {
            OptionalProperty = "some value",
            VersionProperty = new Version(1, 2, 3),
        };

        // Act

        string json = Serialize(classWithOptionalPropertyAndVersionConverter);

        // Assert

        json.ShouldBe("{\"OptionalProperty\":\"some value\",\"VersionProperty\":\"1.2.3\"}");
    }
    
    private class ClassWithOptionalPropertyAndVersionConverter
    {
        public Optional<string> OptionalProperty { get; set; }
        
        [JsonConverter(typeof(VersionConverter))]
        public Version VersionProperty { get; set; }
    }
}
