namespace Harvzor.Optional.JsonConverter.BaseTests;

public abstract class OptionalJsonConverterBaseTests
{
    protected abstract T Deserialize<T>(string str);

    protected abstract string Serialize(object obj);

    #region Single Property
    
    [Fact]
    public void WriteJson_ShouldWriteValue_WhenSinglePropertyIsDefined()
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
    public void WriteJson_ShouldWriteNothing_WhenSinglePropertyIsUndefined()
    {
        // Arrange

        Optional<string?> optionalString = new();
        // Optional should convert in the same way as a normal value.
        string? normalString = default;

        // Act
        
        string optionalJson = Serialize(optionalString);
        string normalJson = Serialize(normalString);
        
        // Assert

        normalJson.ShouldBe("");
        optionalJson.ShouldBe("null");
    }
    
    [Fact]
    public void WriteJson_ShouldWriteNull_WhenSinglePropertyInObjectIsNull()
    {
        // Arrange

        Optional<string?> optionalString = null;
        // Optional should convert in the same way as a normal value.
        string? normalString = null;

        // Act

        string optionalJson = Serialize(optionalString);
        string normalJson = Serialize(normalString);

        // Assert

        normalJson.ShouldBe("null");
        optionalJson.ShouldBe("null");
    }
    
    [Fact]
    public void ReadJson_ShouldReadValue_WhenSinglePropertyIsDefined()
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
    public void ReadJson_ShouldBeUndefined_WhenSinglePropertyIsUndefined()
    {
        // Arrange
        
        string json = "";
        
        // Act
        
        Optional<string> optionalString = Deserialize<Optional<string>>(json);
        // Optional should convert in the same way as a normal value.
        string normalString = Deserialize<string>(json);
        
        // Assert

        optionalString.IsDefined.ShouldBe(false);
        optionalString.Value.ShouldBe(default);
        normalString.ShouldBe(default);
    }
    
    #endregion Single Property

    #region Property In Object
    
    [Fact]
    public void WriteJson_ShouldWriteValue_WhenOptionalPropertyInObjectIsDefined()
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
    public void WriteJson_ShouldWriteNothing_WhenOptionalPropertyInObjectIsUndefined()
    {
        // Arrange

        Foo foo = new Foo();

        // Act

        string json = Serialize(foo);

        // Assert

        json.ShouldBe("{}");
    }
    
    [Fact]
    public void WriteJson_ShouldWriteNull_WhenOptionalPropertyInObjectIsNull()
    {
        // Arrange

        Foo foo = new Foo
        {
            OptionalProperty = null
        };

        // Act

        string json = Serialize(foo);

        // Assert

        json.ShouldBe("{\"OptionalProperty\":null}");
    }

    [Fact]
    public void ReadJson_ShouldBeUndefined_WhenOptionalPropertyInObjectAndNoValue()
    {
        // Arrange
        
        string json = "{}";
        
        // Act
        
        Foo foo = Deserialize<Foo>(json);
        
        // Assert

        foo.OptionalProperty.IsDefined.ShouldBe(false);
        foo.OptionalProperty.Value.ShouldBe(default);
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
    
    [Fact]
    public void ReadJson_ShouldReadCorrectly_WhenOptionalPropertyInObjectHasNullValue()
    {
        // Arrange

        string json = "{\"OptionalProperty\":null}";

        // Act

        Foo foo = Deserialize<Foo>(json);

        // Assert

        foo.OptionalProperty.IsDefined.ShouldBe(true);
        foo.OptionalProperty.Value.ShouldBe(null);
    }
    
    #endregion Property In Object

    private class Foo
    {
        public Optional<string> OptionalProperty { get; set; }
    }
}
