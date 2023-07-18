using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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
    private Type _objectType;
    
    public override bool CanConvert(Type objectType)
    {
        _objectType = objectType;
        
        // TODO: this serializer doesn't effect the property name (at least when it's just a single property and not on an object, unsure on that)
        // TODO: maybe allow this to serialize full objects and defer to default serializers when possible, then I can control the proper name?
        return IsOptional(objectType)
           // TODO: Need to traverse whole tree?
           || (objectType.IsClass && objectType.GetProperties().Any(x => IsOptional(x.PropertyType)));
    }

    public override bool CanRead => IsOptional(_objectType);

    private bool IsOptional(Type objectType)
    {
        return objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Optional<>);
    }

    /// <remarks>
    /// https://stackoverflow.com/a/12680454/
    /// </remarks>
    private MemberInfo[] GetPropertiesAndFields(Type objectType)
    {
        const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance;

        IEnumerable<MemberInfo> fields = objectType.GetFields(bindingFlags);
        PropertyInfo[] properties = objectType.GetProperties(bindingFlags);
        
        return fields
            .Concat(properties)
            .ToArray();
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
        if (value.GetType().IsClass)
            WritePropertyNameJson(writer, value, serializer);
        else if (IsOptional(value.GetType()))
            WritePropertyValueJson(writer, value, serializer);
        else
            throw new Exception("Unexpected value.");
    }

    private void WritePropertyNameJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        writer.WriteStartObject();
        
        // TODO: Need to traverse whole tree?
        foreach (PropertyInfo property in value.GetType().GetProperties())
        {
            if (IsOptional(property.PropertyType))
            {
                if (Attribute.IsDefined(property, typeof(JsonIgnoreAttribute)))
                    continue;

                object optionalValue = property.GetValue(value, null);
                
                if (((IOptional)optionalValue).IsDefined)
                {
                    writer.WritePropertyName(property.Name);
                    serializer.Serialize(writer, optionalValue);
                }
            }
            else
            {
                serializer.Serialize(writer, property);
            }
        }
        
        writer.WriteEndObject();
    }
    
    private void WritePropertyValueJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var optionalType = value.GetType();
        var valueProperty = optionalType.GetProperty(nameof(IOptional.Value))!;
        var isDefinedProperty = optionalType.GetProperty(nameof(IOptional.IsDefined))!;

        var isDefined = (bool)isDefinedProperty.GetValue(value);
        var underlyingValue = valueProperty.GetValue(value);

        if (isDefined)
        {
            // Serialize the underlying value of Optional<T>
            serializer.Serialize(writer, underlyingValue);
        }
    }
}
