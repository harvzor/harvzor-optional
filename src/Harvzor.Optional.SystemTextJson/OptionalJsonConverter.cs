using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

// Missing XML comment for publicly visible type or member
#pragma warning disable CS1591

namespace Harvzor.Optional.SystemTextJson;

/// <summary>
/// Register this customer System.Text.Json converter to get JSON support of <see cref="Optional{T}"/>. Docs: https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/converters-how-to?pivots=dotnet-7-0#register-a-custom-converter
/// </summary>
public class OptionalJsonConverter : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(IOptional).IsAssignableFrom(typeToConvert);
    }

    /// <summary>
    /// Create a converter generically so it can handle the different types of underlying values of <see cref="Optional{T}"/>
    /// without needing to register a different converter for each different underlying type.
    /// </summary>
    public override JsonConverter CreateConverter(
        Type type,
        JsonSerializerOptions options)
    {
        Type valueType = type.GetGenericArguments()[0];

        JsonConverter converter = (JsonConverter)Activator.CreateInstance(
            typeof(OptionalConverterInner<>).MakeGenericType(valueType),
            BindingFlags.Instance | BindingFlags.Public,
            binder: null,
            args: new object[] { options },
            culture: null)!;

        return converter;
    }

    private class OptionalConverterInner<TValue> : JsonConverter<Optional<TValue>>
    {
        private readonly JsonConverter<TValue> _valueConverter;
        private readonly Type _valueType;

        public OptionalConverterInner(JsonSerializerOptions options)
        {
            // For performance, use the existing converter.
            _valueConverter = (JsonConverter<TValue>)options
                .GetConverter(typeof(TValue));

            // Cache the value type.
            _valueType = typeof(TValue);
        }

        public override Optional<TValue> Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            // Get the value.
            TValue value = _valueConverter.Read(ref reader, _valueType, options)!;

            return new Optional<TValue>(value);
        }

        public override void Write(
            Utf8JsonWriter writer,
            Optional<TValue> optional,
            JsonSerializerOptions options)
        {
            // For some reason this line only is it and returns false when it's a single property being serialized, not
            // when it's a property on an object (assuming the TypeInfoResolver is used).
            if (optional.IsDefined)
                _valueConverter.Write(writer, optional.Value, options);
            // Trying to throw an exception here will just break single property serialization:
            // else
            //     throw new InvalidOperationException($"Expected {nameof(OptionalSystemTextJsonExtensions)}.{nameof(OptionalSystemTextJsonExtensions.IgnoreUndefinedOptionals)} to be registered as a modifier, otherwise the property name will be printed in the output JSON.");
        }
    }
}
