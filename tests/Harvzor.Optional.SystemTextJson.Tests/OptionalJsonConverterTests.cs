using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Harvzor.Optional.JsonConverter.BaseTests;

namespace Harvzor.Optional.SystemTextJson.Tests;

public class OptionalJsonConverterTests : OptionalJsonConverterBaseTests
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        IncludeFields = true,
        // todo: ensure that indenting changes the output
        // WriteIndented = true,
        Converters =
        {
            new OptionalJsonConverter()
        },
        TypeInfoResolver = new DefaultJsonTypeInfoResolver
        {
            Modifiers = { OptionalTypeInfoResolverModifiers.IgnoreUndefinedOptionals }
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
        [JsonPropertyName("OptProperty")]
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
