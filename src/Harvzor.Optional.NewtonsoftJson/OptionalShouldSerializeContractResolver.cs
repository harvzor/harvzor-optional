using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Harvzor.Optional.NewtonsoftJson;

public class OptionalShouldSerializeContractResolver : DefaultContractResolver
{
    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        JsonProperty property = base.CreateProperty(member, memberSerialization);
    
        if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Optional<>))
        {
            property.ShouldSerialize =
                instance =>
                {
                    IMember? propertyOrField = instance.GetType().GetPropertyOrField(property.UnderlyingName);

                    if (propertyOrField == null)
                        throw new Exception("Member should surely be found on class?");

                    IOptional optional = (IOptional)propertyOrField.GetValue(instance);
                    return optional.IsDefined;
                };
        }
    
        return property;
    }
}
