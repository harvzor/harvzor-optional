namespace Harvzor.Optional.JsonConverter.BaseTests;

/// <summary>
/// Tests which should be repeated for both of the Newtonsoft and SystemTextJson implementations should be placed in
/// here.
/// </summary>
public abstract class OptionalJsonConverterBaseTests
{
    protected abstract T Deserialize<T>(string str);

    protected abstract string Serialize(object obj);

    #region Single Property
    
    [Fact]
    [Trait("Category","SingleProperty")]
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
    [Trait("Category","SingleProperty")]
    public void WriteJson_ShouldWriteNothing_WhenSinglePropertyIsUndefined()
    {
        // Arrange

        Optional<string?> optionalString = new();

        // Act
        
        string optionalJson = Serialize(optionalString);
        
        // Assert

        optionalJson.ShouldBe("");
        // Don't need to compare to normal property as it would have a value and would act different.
    }
    
    [Fact]
    [Trait("Category","SingleProperty")]
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
    [Trait("Category","SingleProperty")]
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

    [Fact(Skip = "In order for this to work, I have to deserialize to `Optional<string>?`, otherwise it seems to throw an exception out of my control?")]
    [Trait("Category","SingleProperty")]
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
    [Trait("Category","Property In Object")]
    public void WriteJson_ShouldWriteValue_WhenOptionalPropertyInObjectIsDefined()
    {
        // Arrange

        ClassWithOptionalProperty classWithOptionalProperty = new ClassWithOptionalProperty
        {
            OptionalProperty = "some value"
        };

        // Act

        string json = Serialize(classWithOptionalProperty);

        // Assert

        json.ShouldBe("{\"OptionalProperty\":\"some value\"}");
    }
    
    [Fact]
    [Trait("Category","Property In Object")]
    public void WriteJson_ShouldWriteNothing_WhenOptionalPropertyInObjectIsUndefined()
    {
        // Arrange

        ClassWithOptionalProperty classWithOptionalProperty = new ClassWithOptionalProperty();

        // Act

        string json = Serialize(classWithOptionalProperty);

        // Assert

        json.ShouldBe("{}");
    }
    
    [Fact]
    [Trait("Category","Property In Object")]
    public void WriteJson_ShouldWriteNull_WhenOptionalPropertyInObjectIsNull()
    {
        // Arrange

        ClassWithOptionalProperty classWithOptionalProperty = new ClassWithOptionalProperty
        {
            OptionalProperty = null
        };

        // Act

        string json = Serialize(classWithOptionalProperty);

        // Assert

        json.ShouldBe("{\"OptionalProperty\":null}");
    }

    [Fact]
    [Trait("Category","Property In Object")]
    public void ReadJson_ShouldBeUndefined_WhenOptionalPropertyInObjectAndNoValue()
    {
        // Arrange
        
        string json = "{}";
        
        // Act
        
        ClassWithOptionalProperty classWithOptionalProperty = Deserialize<ClassWithOptionalProperty>(json);
        
        // Assert

        classWithOptionalProperty.OptionalProperty.IsDefined.ShouldBe(false);
        classWithOptionalProperty.OptionalProperty.Value.ShouldBe(default);
    }

    [Fact]
    [Trait("Category","Property In Object")]
    public void ReadJson_ShouldReadCorrectly_WhenOptionalPropertyInObject()
    {
        // Arrange

        string json = "{\"OptionalProperty\":\"some value\"}";

        // Act

        ClassWithOptionalProperty classWithOptionalProperty = Deserialize<ClassWithOptionalProperty>(json);

        // Assert

        classWithOptionalProperty.OptionalProperty.IsDefined.ShouldBe(true);
        classWithOptionalProperty.OptionalProperty.Value.ShouldBe("some value");
    }
    
    [Fact]
    [Trait("Category","Property In Object")]
    public void ReadJson_ShouldReadCorrectly_WhenOptionalPropertyInObjectHasNullValue()
    {
        // Arrange

        string json = "{\"OptionalProperty\":null}";

        // Act

        ClassWithOptionalProperty classWithOptionalProperty = Deserialize<ClassWithOptionalProperty>(json);

        // Assert

        classWithOptionalProperty.OptionalProperty.IsDefined.ShouldBe(true);
        classWithOptionalProperty.OptionalProperty.Value.ShouldBe(null);
    }
    
    #endregion Property In Object
    
    #region Field In Object
    
    [Fact]
    [Trait("Category","Field In Object")]
    public void WriteJson_ShouldWriteValue_WhenOptionalFieldInObjectIsDefined()
    {
        // Arrange

        ClassWithOptionalField classWithOptionalField = new ClassWithOptionalField
        {
            OptionalField = "some value"
        };

        // Act

        string json = Serialize(classWithOptionalField);

        // Assert

        json.ShouldBe("{\"OptionalField\":\"some value\"}");
    }
    
    [Fact]
    [Trait("Category","Field In Object")]
    public void WriteJson_ShouldWriteNothing_WhenOptionalFieldInObjectIsUndefined()
    {
        // Arrange

        ClassWithOptionalField classWithOptionalField = new ClassWithOptionalField();

        // Act

        string json = Serialize(classWithOptionalField);

        // Assert

        json.ShouldBe("{}");
    }
    
    [Fact]
    [Trait("Category","Field In Object")]
    public void WriteJson_ShouldWriteNull_WhenOptionalFieldInObjectIsNull()
    {
        // Arrange

        ClassWithOptionalField classWithOptionalField = new ClassWithOptionalField
        {
            OptionalField = null
        };

        // Act

        string json = Serialize(classWithOptionalField);

        // Assert

        json.ShouldBe("{\"OptionalField\":null}");
    }

    [Fact]
    [Trait("Category","Field In Object")]
    public void ReadJson_ShouldBeUndefined_WhenOptionalFieldInObjectAndNoValue()
    {
        // Arrange
        
        string json = "{}";
        
        // Act
        
        ClassWithOptionalField classWithOptionalField = Deserialize<ClassWithOptionalField>(json);
        
        // Assert

        classWithOptionalField.OptionalField.IsDefined.ShouldBe(false);
        classWithOptionalField.OptionalField.Value.ShouldBe(default);
    }

    [Fact]
    [Trait("Category","Field In Object")]
    public void ReadJson_ShouldReadCorrectly_WhenOptionalFieldInObject()
    {
        // Arrange

        string json = "{\"OptionalField\":\"some value\"}";

        // Act

        ClassWithOptionalField classWithOptionalField = Deserialize<ClassWithOptionalField>(json);

        // Assert

        classWithOptionalField.OptionalField.IsDefined.ShouldBe(true);
        classWithOptionalField.OptionalField.Value.ShouldBe("some value");
    }
    
    [Fact]
    [Trait("Category","Field In Object")]
    public void ReadJson_ShouldReadCorrectly_WhenOptionalFieldInObjectHasNullValue()
    {
        // Arrange

        string json = "{\"OptionalField\":null}";

        // Act

        ClassWithOptionalField classWithOptionalField = Deserialize<ClassWithOptionalField>(json);

        // Assert

        classWithOptionalField.OptionalField.IsDefined.ShouldBe(true);
        classWithOptionalField.OptionalField.Value.ShouldBe(null);
    }
    
    #endregion Field In Object

    #region Property In Nested Object

    [Fact]
    [Trait("Category","Property In Nested Object")]
    public void WriteJson_ShouldWriteValue_WhenOptionalPropertyInNestedObjectIsDefined()
    {
        // Arrange

        NestedClass nestedClass = new NestedClass
        {
            ClassWithOptionalProperty = new()
            {
                OptionalProperty = "some value"
            }
        };

        // Act

        string json = Serialize(nestedClass);

        // Assert

        json.ShouldBe("{\"ClassWithOptionalProperty\":{\"OptionalProperty\":\"some value\"}}");
    }
    
    [Fact]
    [Trait("Category","Property In Nested Object")]
    public void WriteJson_ShouldWriteValue_WhenOptionalPropertyInNestedObjectIsUndefined()
    {
        // Arrange

        NestedClass nestedClass = new NestedClass
        {
            ClassWithOptionalProperty = new()
        };

        // Act

        string json = Serialize(nestedClass);

        // Assert

        json.ShouldBe("{\"ClassWithOptionalProperty\":{}}");
    }
    
    [Fact]
    [Trait("Category","Property In Nested Object")]
    public void WriteJson_ShouldWriteValue_WhenOptionalPropertyInNestedObjectIsNull()
    {
        // Arrange

        NestedClass nestedClass = new NestedClass
        {
            ClassWithOptionalProperty = new()
            {
                OptionalProperty = null
            }
        };

        // Act

        string json = Serialize(nestedClass);

        // Assert

        json.ShouldBe("{\"ClassWithOptionalProperty\":{\"OptionalProperty\":null}}");
    }
    
    [Fact]
    [Trait("Category","Property In Nested Object")]
    public void ReadJson_ShouldBeUndefined_WhenOptionalPropertyInNestedObjectAndNoValue()
    {
        // Arrange
        
        string json = "{\"ClassWithOptionalProperty\":{}}";
        
        // Act
        
        NestedClass nestedClass = Deserialize<NestedClass>(json);
        
        // Assert

        nestedClass.ClassWithOptionalProperty.OptionalProperty.IsDefined.ShouldBe(false);
        nestedClass.ClassWithOptionalProperty.OptionalProperty.Value.ShouldBe(default);
    }

    [Fact]
    [Trait("Category","Property In Nested Object")]
    public void ReadJson_ShouldReadCorrectly_WhenOptionalPropertyInNestedObject()
    {
        // Arrange

        string json = "{\"ClassWithOptionalProperty\":{\"OptionalProperty\":\"some value\"}}";

        // Act

        NestedClass nestedClass = Deserialize<NestedClass>(json);

        // Assert

        nestedClass.ClassWithOptionalProperty.OptionalProperty.IsDefined.ShouldBe(true);
        nestedClass.ClassWithOptionalProperty.OptionalProperty.Value.ShouldBe("some value");
    }
    
    [Fact]
    [Trait("Category","Property In Nested Object")]
    public void ReadJson_ShouldReadCorrectly_WhenOptionalPropertyInNestedObjectHasNullValue()
    {
        // Arrange

        string json = "{\"ClassWithOptionalProperty\":{\"OptionalProperty\":null}}";

        // Act

        NestedClass nestedClass = Deserialize<NestedClass>(json);

        // Assert

        nestedClass.ClassWithOptionalProperty.OptionalProperty.IsDefined.ShouldBe(true);
        nestedClass.ClassWithOptionalProperty.OptionalProperty.Value.ShouldBe(null);
    }

    #endregion Property In Nested Object
    
    #region Optional In Collection

    [Fact]
    [Trait("Category","Optional In Collections")]
    public void WriteJson_ShouldWriteCorrectly_WhenUndefinedAndDefinedAndNullValuesInCollection()
    {
        // Arrange

        var collection = new List<Optional<string?>>
        {
            new(),
            new(null),
            new("some value"),
        };

        // Act

        string json = Serialize(collection);

        // Assert

        json.ShouldBe("[null,\"some value\"]");
    }
    
    [Fact]
    [Trait("Category","Optional In Collections")]
    public void ReadJson_ShouldReadCorrectly_WhenNullAndDefinedInCollection()
    {
        // Arrange

        string json = "[null,\"some value\"]";

        // Act

        List<Optional<string?>> collection = Deserialize<List<Optional<string?>>>(json);

        // Assert

        collection[0].IsDefined.ShouldBe(true);
        collection[0].Value.ShouldBe(null);
        
        collection[1].IsDefined.ShouldBe(true);
        collection[1].Value.ShouldBe("some value");
    }
    
    #endregion Optional In Collection

    private class ClassWithOptionalProperty
    {
        public Optional<string> OptionalProperty { get; set; }
    }
    
    private class ClassWithOptionalField
    {
        public Optional<string> OptionalField;
    }

    private class NestedClass
    {
        public ClassWithOptionalProperty ClassWithOptionalProperty { get; set; }
    }
}
