using System;
using System.Linq;
using Newtonsoft.Json;

// Missing XML comment for publicly visible type or member
#pragma warning disable CS1591

namespace Harvzor.Optional.NewtonsoftJson;

/// <summary>
/// Register this customer Newtonsoft converter to get JSON support of <see cref="Optional{T}"/>. Docs: https://www.newtonsoft.com/json/help/html/CustomJsonConverter.htm
/// </summary>
/// <remarks>
/// Dev note: It's generally better to use `JsonConverter&lt;T&gt;` because that only has to convert a specific property,
/// but then when registering the converters, you'd have to specify each type that might be converted (e.g. OptionalJsonConverterr&lt;string&gt;).
/// </remarks>
public class OptionalJsonConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return IsOptional(objectType);
    }

    private bool IsOptional(Type objectType)
    {
        return objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Optional<>);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var optionalInstance = Activator.CreateInstance(objectType);
        var valueProperty = objectType.GetProperty(nameof(IOptional.Value))!;
        
        valueProperty.SetValue(optionalInstance, reader.Value);

        return optionalInstance;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var optionalType = value.GetType();
        var valueProperty = optionalType.GetProperty("Value")!;
        var isDefinedProperty = optionalType.GetProperty("IsDefined")!;

        var isDefined = (bool)isDefinedProperty.GetValue(value);
        var underlyingValue = valueProperty.GetValue(value);

        if (isDefined)
        {
            // Serialize the underlying value of Optional<T>
            serializer.Serialize(writer, underlyingValue);
        }
    }
}
