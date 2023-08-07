using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Harvzor.Optional.NewtonsoftJson;

/// <summary>
/// Ensures that only properties with defined values are printed in the output JSON when serializing. Without this, the
/// property name will still be serialized, but without a value.
/// https://github.com/JamesNK/Newtonsoft.Json/issues/1859#issuecomment-463061596
/// </summary>
public class OptionalShouldSerializeContractResolver : DefaultContractResolver
{
    /// <inheritdoc />
    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        JsonProperty property = base.CreateProperty(member, memberSerialization);
    
        if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Optional<>))
        {
            property.ShouldSerialize =
                instance =>
                {
                    IMember propertyOrField = instance.GetType().GetPropertyOrField(property.UnderlyingName)!;

                    IOptional optional = (IOptional)propertyOrField.GetValue(instance);
                    return optional.IsDefined;
                };
        }
    
        return property;
    }
}
