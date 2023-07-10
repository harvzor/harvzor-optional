namespace Harvzor.Optional.JsonConverter.BaseTests;

public abstract class OptionalJsonConverterBaseTests
{
    protected abstract T Deserialize<T>(string str);

    protected abstract string Serialize(object obj);

    [Fact]
    public void WriteJson_ShouldWriteCorrectly_WhenDefinedAndSingleProperty()
    {
        // Arrange

        Optional<string> optionalString = "some value";
        // Optional should convert in the same way as a normal value.
        string normalString = "some value";

        // Act
        
        string optionalJson = Serialize(optionalString);
        string normalJson = Serialize(normalString);
        
        // Assert

        normalJson.ShouldBe("\"some value\"");
        optionalJson.ShouldBe("\"some value\"");
    }
    
    [Fact]
    public void ReadJson_ShouldReadCorrectly_WhenSingleProperty()
    {
        // Arrange
        
        string json = "\"some value\"";
        
        // Act
        
        Optional<string> optionalString = Deserialize<Optional<string>>(json);
        // Optional should convert in the same way as a normal value.
        string normalString = Deserialize<string>(json);
        
        // Assert

        optionalString.Value.ShouldBe("some value");
        normalString.ShouldBe("some value");
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
