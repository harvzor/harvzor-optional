using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Linq;

// Missing XML comment for publicly visible type or member
#pragma warning disable CS1591

namespace Harvzor.Optional.NewtonsoftJson;

/// <summary>
/// Register this customer Newtonsoft converter to get JSON support of <see cref="Optional{T}"/>. Docs: https://www.newtonsoft.com/json/help/html/CustomJsonConverter.htm
/// </summary>
/// <remarks>
/// Dev note: It's generally better to use `JsonConverter&lt;T&gt;` because that only has to convert a specific property.
/// However, there was an issue with that so I had to use the more basic `JsonConverter` which accesses all properties
/// on the JSON object.
/// </remarks>
public class OptionalJsonConverter : JsonConverter
{
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        writer.WriteStartObject();

        JsonObjectContract jsonContract = (JsonObjectContract)serializer.ContractResolver.ResolveContract(value.GetType());

        foreach (PropertyInfo propertyInfo in value.GetType().GetProperties())
        {
            JsonProperty jsonProperty = jsonContract.Properties.First(x => x.UnderlyingName == propertyInfo.Name);

            if (jsonProperty.Ignored)
            {
                continue;
            }

            string jsonPropertyName = jsonProperty.PropertyName;
            object propertyValue = propertyInfo.GetValue(value);

            //Resolve optional wrapper
            if (typeof(IOptional).IsAssignableFrom(propertyInfo.PropertyType))
            {
                IOptional optionalProperty = (IOptional)propertyValue;

                //Skip property if no value is set
                if (optionalProperty.IsDefined == false)
                {
                    continue;
                }

                //Update property value and type
                propertyValue = optionalProperty.Value;
            }

            //Write property name
            writer.WritePropertyName(jsonPropertyName);

            //Write property value
            serializer.Serialize
            (
                jsonWriter: writer,
                value: propertyValue
            );
        }

        writer.WriteEndObject();
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType.GetProperties().Any(p => typeof(IOptional).IsAssignableFrom(p.PropertyType));
    }

    public override bool CanWrite => true;
    public override bool CanRead => false;
}