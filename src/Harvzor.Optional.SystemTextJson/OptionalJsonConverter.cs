using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

// Missing XML comment for publicly visible type or member
#pragma warning disable CS1591

namespace Harvzor.Optional.SystemTextJson;

/// <summary>
/// Register this customer System.Text.Json converter to get JSON support of <see cref="Optional{T}"/>. Docs: https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/converters-how-to?pivots=dotnet-7-0#register-a-custom-converter
/// </summary>
public class OptionalJsonConverter : JsonConverter<object>
{
    public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        object result = (object)Activator.CreateInstance(typeToConvert);

        using (JsonDocument document = JsonDocument.ParseValue(ref reader))
        {
            var jsonProperties = document.RootElement.EnumerateObject()
                .ToDictionary(x => x.Name.ToLower());

            foreach (PropertyInfo propertyInfo in typeToConvert.GetProperties())
            {
                if (propertyInfo.GetCustomAttribute<JsonIgnoreAttribute>() != null)
                {
                    continue;
                }

                bool isOptional = typeof(IOptional).IsAssignableFrom(propertyInfo.PropertyType);

                Type propertyType = isOptional
                    ? ((IOptional)propertyInfo.GetValue(result)).GenericType
                    : propertyInfo.PropertyType;

                string jsonPropertyName = propertyInfo.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name.ToLower()
                                          ?? propertyInfo.Name.ToLower();

                if (jsonProperties.ContainsKey(jsonPropertyName))
                {
                    JsonProperty jsonProperty = jsonProperties[jsonPropertyName];

                    if (jsonProperty.Value.ValueKind != JsonValueKind.Undefined)
                    {
                        object value = JsonSerializer.Deserialize(jsonProperty.Value.GetRawText(), propertyType, options);
                            
                        if (isOptional)
                        {
                            IOptional optional = (IOptional)propertyInfo.GetValue(result);
                            optional.Value = value;
                            propertyInfo.SetValue(result, optional);
                        }
                        else
                        {
                            propertyInfo.SetValue(result, value);
                        }
                    }
                }
            }
        }

        return result;
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        //Loop through all properties of the IPatchableModel
        foreach (PropertyInfo propertyInfo in value.GetType().GetProperties())
        {
            if (propertyInfo.GetCustomAttribute<JsonIgnoreAttribute>() != null)
            {
                continue;
            }

            string jsonPropertyName = propertyInfo.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name
                                      ?? options.PropertyNamingPolicy?.ConvertName(propertyInfo.Name)
                                      ?? propertyInfo.Name;
            object propertyValue = propertyInfo.GetValue(value);
            Type propertyType = propertyInfo.PropertyType;

            //Resolve optional wrapper
            if (typeof(IOptional).IsAssignableFrom(propertyType))
            {
                IOptional optionalProperty = (IOptional)propertyValue;

                //Skip property if no value is set
                if (optionalProperty.IsDefined == false)
                {
                    continue;
                }

                //Update property value and type
                propertyValue = optionalProperty.Value;
                propertyType = optionalProperty.Value.GetType();
            }

            //Write property name
            writer.WritePropertyName(jsonPropertyName);

            //Write property value
            JsonSerializer.Serialize
            (
                writer: writer,
                value: propertyValue,
                inputType: propertyType,
                options: options
            );
        }

        writer.WriteEndObject();
    }
        
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.GetProperties().Any(p => typeof(IOptional).IsAssignableFrom(p.PropertyType));
    }
}