using System.Text.Json;

namespace Harvzor.Optional.SystemTextJson.Tests;

public class OptionalJsonConverterTests
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        Converters =
        {
            new OptionalJsonConverter()
        }
    };

    private T Deserialize<T>(string str)
    {
        return JsonSerializer.Deserialize<T>(str, _jsonSerializerOptions)!;
    }
    
    private string Serialize(object obj)
    {
        return JsonSerializer.Serialize(obj, _jsonSerializerOptions);
    }
    
    [Fact]
    public void WriteJson_ShouldWriteCorrectly_WhenDefinedAndSingleProperty()
    {
        // Arrange

        Optional<string> optionalString = "some value";

        // Act
        
        string json = Serialize(optionalString)!;
        
        // Assert

        json.ShouldBe("");
    }
    
    [Fact]
    public void ReadJson_ShouldReadCorrectly_WhenSingleProperty()
    {
        // Arrange
        
        string json = "{\"DefinedProperty\":\"some value\"}";
        
        // Act
        
        Optional<string> test = Deserialize<Optional<string>>(json);
        
        // Assert

        test.Value.ShouldBe("some value");
    }

    [Fact]
    public void WriteJson_ShouldWriteCorrectly_WhenOptionalPropertyInObject()
    {
        // Arrange

        Foo foo = new Foo
        {
            OptionalProperty = "some value"
        };

        // Act

        string json = Serialize(foo);

        // Assert

        json.ShouldBe("{\"OptionalProperty\":\"some value\"}");
    }
    
    [Fact]
    public void ReadJson_ShouldReadCorrectly_WhenOptionalPropertyInObject()
    {
        // Arrange

        string json = "{\"OptionalProperty\":\"some value\"}";

        // Act

        Foo foo = Deserialize<Foo>(json);

        // Assert

        foo.OptionalProperty.IsDefined.ShouldBe(true);
        foo.OptionalProperty.Value.ShouldBe("some value");
    }

    private class Foo
    {
        public Optional<string> OptionalProperty { get; set; }
    }
}
