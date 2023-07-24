using Harvzor.Optional.JsonConverter.BaseTests;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

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
            // Isn't needed as the contract resolver only effects writing:
            // ContractResolver = new OptionalShouldSerializeContractResolver(),
        })!;
    }

    protected override string Serialize(object obj)
    {
        return JsonConvert.SerializeObject(obj, new JsonSerializerSettings()
        {
            // TODO: setting to indented doesn't change the output as the output has been overwritten...
            // Formatting = Formatting.Indented,
            Converters = new List<Newtonsoft.Json.JsonConverter>
            {
                new OptionalJsonConverter(),
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
    [Trait("Category","Integrates With Other Converters / Contract Resolvers")]
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
    
    [Fact]
    [Trait("Category","Integrates With Other Converters / Contract Resolvers")]
    public void WriteJson_ShouldWriteValue_WhenMultipleContractResolversAreUsed()
    {
        // Arrange

        ClassWithOptionalPropertyAndDateTime classWithOptionalPropertyAndDateTime = new ClassWithOptionalPropertyAndDateTime
        {
            OptionalProperty = new DateTime(2023, 01, 01),
            OptionalPropertyTwo = new Optional<DateTime>(),
            DateTime = new DateTime(2022, 01, 01),
        };

        // Act
        
        string json = JsonConvert.SerializeObject(classWithOptionalPropertyAndDateTime, new JsonSerializerSettings()
        {
            Converters = new List<Newtonsoft.Json.JsonConverter>
            {
                new OptionalJsonConverter(),
            },
            ContractResolver = new DateTimeMixedWithOptionalContractResolver(),
        });

        // Assert

        json.ShouldBe("{\"OptionalProperty\":new Date(1672527600000),\"DateTime\":new Date(1640991600000)}");
    }
    
    private class ClassWithOptionalPropertyAndDateTime
    {
        public Optional<DateTime> OptionalProperty { get; set; }
        
        public Optional<DateTime> OptionalPropertyTwo { get; set; }
        
        public DateTime DateTime { get; set; }
    }
    
    /// <summary>
    /// https://www.newtonsoft.com/json/help/html/contractresolver.htm
    /// </summary>
    /// <remarks>
    /// This method (of inheriting from other resolvers) doesn't allow you to use a built in contract resolvers, maybe
    /// there's a better way of compositing resolvers here:
    /// https://stackoverflow.com/questions/39612636/add-multiple-contract-resolver-in-newtonsoft-json
    /// </remarks>
    private class DateTimeMixedWithOptionalContractResolver : OptionalShouldSerializeContractResolver
    {
        protected override JsonContract CreateContract(Type objectType)
        {
            JsonContract contract = base.CreateContract(objectType);

            // this will only be called once and then cached
            if (objectType == typeof(DateTime) || objectType == typeof(DateTimeOffset))
            {
                contract.Converter = new JavaScriptDateTimeConverter();
            }

            return contract;
        }
    }
}
