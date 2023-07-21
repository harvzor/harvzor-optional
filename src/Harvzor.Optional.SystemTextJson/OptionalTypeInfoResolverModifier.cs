using System;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Harvzor.Optional.SystemTextJson;

/// <summary>
/// Modifiers for the <see cref="JsonSerializerOptions.TypeInfoResolver"/> which change how <see cref="Optional{T}"/>
/// types are serialized.
/// </summary>
public static class OptionalTypeInfoResolverModifiers
{
    /// <summary>
    /// Ignore properties and fields that are <see cref="Optional{T}"/> and haven't been defined.
    /// This ensures that the property is never printed in the output JSON when serializing.
    /// Without this, the <see cref="OptionalJsonConverter"/> still prints the property name.
    /// <br/>
    /// <br/>
    /// Another option is to decorate all properties that are using <see cref="Optional{T}"/> with `[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]`.
    /// </summary>
    /// <remarks>
    /// https://devblogs.microsoft.com/dotnet/system-text-json-in-dotnet-7/#example-conditional-serialization
    /// Requires System.Text.Json v7.
    /// </remarks>
    /// <example>
    /// new JsonSerializerOptions()
    /// {
    ///     Converters =
    ///     {
    ///         new OptionalJsonConverter()
    ///     },
    ///     TypeInfoResolver = new DefaultJsonTypeInfoResolver
    ///     {
    ///         Modifiers = { OptionalTypeInfoResolverModifiers.IgnoreUndefinedOptionals }
    ///     }
    /// }
    /// </example>
    public static void IgnoreUndefinedOptionals(JsonTypeInfo typeInfo)
    {
        bool CanConvert(Type type) => typeof(IOptional).IsAssignableFrom(type);

        void AddShouldSerialize(JsonPropertyInfo propertyInfo)
        {
            propertyInfo.ShouldSerialize = static (_, value) =>
                ((IOptional)value!).IsDefined;
        }

        if (CanConvert(typeInfo.Type))
            AddShouldSerialize(typeInfo.CreateJsonPropertyInfo(typeof(IOptional), "asd"));
        else
        {
            // When `IncludeFields = true` is set, it seems that System.Text.Json treats properties as fields,
            // so no need to write anything fancy for fields here.
            foreach (JsonPropertyInfo propertyInfo in typeInfo.Properties)
                if (CanConvert(propertyInfo.PropertyType))
                    AddShouldSerialize(propertyInfo);
        }
    }
}
