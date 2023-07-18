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
           || (objectType.IsClass && objectType.GetPropertiesAndFields().Any(member => IsOptional(member.GetMemberType())));
    }

    public override bool CanRead => IsOptional(_objectType);

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
        foreach (IMember member in value.GetType().GetPropertiesAndFields())
        {
            if (IsOptional(member.GetMemberType()))
            {
                if (Attribute.IsDefined(member.GetMemberInfo(), typeof(JsonIgnoreAttribute)))
                    continue;

                object optionalValue = member.GetValue(value);

                if (((IOptional)optionalValue).IsDefined)
                {
                    writer.WritePropertyName(member.Name);
                    serializer.Serialize(writer, optionalValue);
                }
            }
            else
            {
                serializer.Serialize(writer, member);
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
