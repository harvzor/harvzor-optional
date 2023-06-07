using System;
using System.Reflection;
using Newtonsoft.Json;
using Harvzor.Optional;
using Newtonsoft.Json.Serialization;
using System.Linq;

namespace Harvzor.Optional.NewtonsoftJson
{
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
}