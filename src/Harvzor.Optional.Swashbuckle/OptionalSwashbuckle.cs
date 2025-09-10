using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Harvzor.Optional.Swashbuckle;

public static class OptionalSwashbuckle
{
    /// <summary>
    /// Remaps a <see cref="Optional{T}"/> to its T argument, and also ensures that the T argument is in the schema repo.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="optionalType">Should be of type <see cref="Optional{T}"/>.</param>
    private static void RemapOptionalObject(this SwaggerGenOptions options, Type optionalType)
    {
        // Create OptionalDocumentFilter<T>.
        Type CreateGenericOptionalDocumentFilter(Type argumentType)
        {
            Type optionalDocumentFilterType = typeof(OptionalDocumentFilter<>);
            return optionalDocumentFilterType.MakeGenericType(argumentType);
        }
        
        // Invoke `options.DocumentFilter<OptionalDocumentFilter<T>>()` but with `genericType` instead of `<T>`;
        void InvokeDocumentFilter(Type genericOptionalDocumentFilter)
        {
            MethodInfo documentFilterMethod = typeof(SwaggerGenOptionsExtensions)
                .GetMethod(nameof(SwaggerGenOptionsExtensions.DocumentFilter))!;

            MethodInfo genericDocumentFilterMethod = documentFilterMethod.MakeGenericMethod(genericOptionalDocumentFilter);

            genericDocumentFilterMethod.Invoke(null, new object?[] { options, Array.Empty<object>() });
        }

        Type genericType = optionalType.GetGenericArguments().First();

        bool isNullable = genericType.IsGenericType
            && genericType.GetGenericTypeDefinition() == typeof(Nullable<>);
        
        if (isNullable)
            genericType = genericType.GetGenericArguments().First();

        // Ensure the same type isn't mapped twice somehow otherwise `options.MapType` will throw.
        if (options.SchemaGeneratorOptions.CustomTypeMappings.Any(x => x.Key == optionalType))
            return;
        
        // Mapping this will strangely mean that Optional<T> is not picked up in the SchemaFilter, though other
        // types like Optional<int> are (even if they've been mapped!).
        options.MapType(optionalType, () => new OpenApiSchema
        {
            Type = "object",
            // Seems that Swagger doesn't mark references as nullable ever?
            // Nullable = isNullable,
            Reference = new OpenApiReference
            {
                Id = genericType.Name,
                Type = ReferenceType.Schema,
            }
        });

        // This should do the same as `options.DocumentFilter<OptionalDocumentFilter<T>>();`
        Type genericDocumentFilterType = CreateGenericOptionalDocumentFilter(genericType);
        InvokeDocumentFilter(genericDocumentFilterType);
    }
    
    private static void FixMappingsForUsedOptionalsInAssembly(this SwaggerGenOptions options, Assembly assembly)
    {
        Type openGenericOptionalType = typeof(Optional<>);
        
        HashSet<Type> visitedTypes = new();
        
        // Recursively find all the Optional<T> properties on the given type.
        IEnumerable<Type> WalkPropertiesAndFindOptionalProperties(Type type)
        {
            // Prevent recursion loop.
            if (!visitedTypes.Add(type))
                yield break;

            Type nonOptionalType = type.IsGenericType && type.GetGenericTypeDefinition() == openGenericOptionalType
                ? type.GetGenericArguments().First()
                : type;

            Type[] propertyTypes = nonOptionalType
                .GetProperties()
                .Select(property => property.PropertyType)
                .ToArray();

            foreach (Type propertyType in propertyTypes)
            {
                foreach (Type item in WalkPropertiesAndFindOptionalProperties(propertyType))
                    yield return item;

                if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == openGenericOptionalType)
                    yield return propertyType;
            }
        }
        
        // Find any Optional<T>'s.
        IEnumerable<Type> typesUsingOptional = assembly
            .GetTypes()
            .Where(type => type.IsSubclassOf(typeof(ControllerBase)))
            .SelectMany(assemblyType =>
            {
                // Find parameters on controller methods like `Foo foo` on `public Foo Post(Foo foo)`:
                Type[] parameters = assemblyType
                    // This doesn't work with a minimal API like:
                    // `app.MapPost("/", (Optional<Foo> foo) => foo);`
                    // as it doesn't look like Optional<Foo> is defined in the assembly?
                    .GetMethods()
                    .Where(m => m.DeclaringType == assemblyType)
                    .SelectMany(method =>
                    {
                        // Find parameters.
                        IEnumerable<Type> types = method
                            .GetParameters()
                            .Select(x => x.ParameterType);
                        
                        // Find return type according to Swashbuckle attributes.
                        IEnumerable<ProducesResponseTypeAttribute> producesResponseTypeAttribute = method
                            .GetCustomAttributes<ProducesResponseTypeAttribute>()
                            .ToArray();

                        if (producesResponseTypeAttribute.Any())
                            types = types.Concat(producesResponseTypeAttribute.Select(x => x.Type));

                        return types
                            // Find return type:
                            .Concat(new[] { method.ReturnType });
                    })
                    .ToArray();
                
                Type[] optionalParameters = parameters
                    .Where(parameterType => parameterType.IsGenericType
                        && parameterType.GetGenericTypeDefinition() == openGenericOptionalType
                    )
                    .ToArray();

                IEnumerable<Type> optionalPropertyTypes = parameters.SelectMany(WalkPropertiesAndFindOptionalProperties);

                return optionalParameters.Concat(optionalPropertyTypes);
            })
            .Distinct();
        
        // Fix the mappings for Optional<T>'s:
        foreach (Type typeUsingOptional in typesUsingOptional)
            options.FixOptionalMappingForType(typeUsingOptional);
    }
    
    /// <summary>
    /// Fix the mapping for a <see cref="Optional{T}"/> type.
    /// </summary>
    /// <typeparam name="T">Pass in a <see cref="Optional{T}"/> type.</typeparam>
    /// <param name="options">Swagger options you get access to when calling `services.AddSwaggerGen(options => {});`.</param>
    public static SwaggerGenOptions FixOptionalMappingForType<T>(this SwaggerGenOptions options)
    {
        return options.FixOptionalMappingForType(typeof(T));
    }
    
    /// <summary>
    /// Fix the mapping for a <see cref="Optional{T}"/> type.
    /// </summary>
    /// <param name="options">Swagger options you get access to when calling `services.AddSwaggerGen(options => {});`.</param>
    /// <param name="typeUsingOptional">Pass in a <see cref="Optional{T}"/> type.</param>
    public static SwaggerGenOptions FixOptionalMappingForType(this SwaggerGenOptions options, Type typeUsingOptional)
    {
        if (!typeUsingOptional.IsGenericType || typeUsingOptional.GetGenericTypeDefinition() != typeof(Optional<>))
            throw new ArgumentException("Type must be Optional<T>.", nameof(typeUsingOptional));
        
        Type argumentType = typeUsingOptional.GetGenericArguments().First();
        
        GetTypeAndFormat(argumentType, out DataType argumentOpenApiType, out string? argumentFormat);
        
        if (argumentOpenApiType == DataType.Object)
        {
            options.RemapOptionalObject(typeUsingOptional);
        }
        else if (argumentOpenApiType == DataType.Array)
        {
            options.MapType(typeUsingOptional, () => GetSchemaOfArray(argumentType));
        }
        // Normal types:
        else
        {
            options.MapType(typeUsingOptional, () => new OpenApiSchema
            {
                Type = argumentOpenApiType.ToString().ToLower(),
                Format = argumentFormat,
                // Nullable only seems to work when it's a single property, not when it's a property in an object?
                // todo: create example project showing Nullable doesn't seem to work
                // Nullable = argumentType.IsReferenceOrNullableType() // Should catch `string` and `int?`.
                    // || (
                    //     argumentType.IsGenericType
                    //     && argumentType.GetGenericTypeDefinition() == typeof(Nullable<>)
                    // ),
            });
        }

        return options;
    }

    private static OpenApiSchema GetSchemaOfArray(Type argumentType)
    {
        Type arrayType;
            
        // object[] will pass:
        if (argumentType.IsArray)
            arrayType = argumentType.GetElementType()!;
        // object[] would also pass here so the order is important here:
        else if (
            (argumentType.IsGenericType && argumentType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            || argumentType.GetInterfaces().Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>))
        )
        {
            arrayType = argumentType.GetGenericArguments()[0];
        }
        else
        {
            throw new ArgumentException("Expected argument to be of type IEnumerable.", nameof(argumentType));
        }
        
        GetTypeAndFormat(arrayType, out DataType arrayDataType, out string? arrayFormat);

        // If it's a multidimensional array...
        if (arrayDataType == DataType.Array)
        {
            return new OpenApiSchema
            {
                Type = "array",
                Items = GetSchemaOfArray(arrayType)
            };
        }

        return new OpenApiSchema
        {
            Type = "array",
            Items = new OpenApiSchema
            {
                Type = arrayDataType.ToString().ToLower(),
                Format = arrayFormat,
                Nullable = arrayType.IsReferenceOrNullableType()
            }
        };
    }

    /// <remarks>
    /// Should support same list as https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/8f363f7359cb1cb8fa5de5195ec6d97aefaa16b3/src/Swashbuckle.AspNetCore.SwaggerGen/SchemaGenerator/JsonSerializerDataContractResolver.cs#L224.
    /// </remarks>
    private static void GetTypeAndFormat(Type argumentType, out DataType argumentOpenApiType, out string? argumentFormat)
    {
        argumentFormat = null;
            
        if (argumentType == typeof(bool) || argumentType == typeof(bool?))
        {
            argumentOpenApiType = DataType.Boolean;
        }
        else if (argumentType == typeof(byte) || argumentType == typeof(byte?))
        {
            argumentOpenApiType = DataType.Integer;
            argumentFormat = "int32";
        }
        else if (argumentType == typeof(sbyte) || argumentType == typeof(sbyte?))
        {
            argumentOpenApiType = DataType.Integer;
            argumentFormat = "int32";
        }
        else if (argumentType == typeof(short) || argumentType == typeof(short?))
        {
            argumentOpenApiType = DataType.Integer;
            argumentFormat = "int32";
        }
        else if (argumentType == typeof(ushort) || argumentType == typeof(ushort?))
        {
            argumentOpenApiType = DataType.Integer;
            argumentFormat = "int32";
        }
        else if (argumentType == typeof(int) || argumentType == typeof(int?))
        {
            argumentOpenApiType = DataType.Integer;
            argumentFormat = "int32";
        }
        else if (argumentType == typeof(uint) || argumentType == typeof(uint?))
        {
            argumentOpenApiType = DataType.Integer;
            argumentFormat = "int32";
        }
        else if (argumentType == typeof(long) || argumentType == typeof(long?))
        {
            argumentOpenApiType = DataType.Integer;
            argumentFormat = "int64";
        }
        else if (argumentType == typeof(ulong) || argumentType == typeof(ulong?))
        {
            argumentOpenApiType = DataType.Integer;
            argumentFormat = "int64";
        }
        else if (argumentType == typeof(float) || argumentType == typeof(float?))
        {
            argumentOpenApiType = DataType.Number;
            argumentFormat = "float";
        }
        else if (argumentType == typeof(double) || argumentType == typeof(double?))
        {
            argumentOpenApiType = DataType.Number;
            argumentFormat = "double";
        }
        else if (argumentType == typeof(decimal) || argumentType == typeof(decimal?))
        {
            argumentOpenApiType = DataType.Number;
            argumentFormat = "double";
        }
        else if (argumentType == typeof(string) /* || argumentType == typeof(string?) */)
        {
            argumentOpenApiType = DataType.String;
        }
        else if (argumentType == typeof(char) || argumentType == typeof(char?))
        {
            argumentOpenApiType = DataType.String;
        }
        else if (argumentType == typeof(byte[]) /* || argumentType == typeof(byte[]?) */)
        {
            argumentOpenApiType = DataType.String;
            argumentFormat = "byte";
        }
        else if (argumentType == typeof(DateTime) || argumentType == typeof(DateTime?))
        {
            argumentOpenApiType = DataType.String;
            argumentFormat = "date-time";
        }
        else if (argumentType == typeof(DateTimeOffset) || argumentType == typeof(DateTimeOffset?))
        {
            argumentOpenApiType = DataType.String;
            argumentFormat = "date-time";
        }
        else if (argumentType == typeof(Guid) || argumentType == typeof(Guid?))
        {
            argumentOpenApiType = DataType.String;
            argumentFormat = "uuid";
        }
        else if (argumentType == typeof(Uri) /* || argumentType == typeof(Uri?) */)
        {
            argumentOpenApiType = DataType.String;
            argumentFormat = "uri";
        }
#if NET6_0_OR_GREATER
        else if (argumentType == typeof(DateOnly) || argumentType == typeof(DateOnly?))
        {
            argumentOpenApiType = DataType.String;
            argumentFormat = "date";
        }
        else if (argumentType == typeof(TimeOnly) || argumentType == typeof(TimeOnly?))
        {
            argumentOpenApiType = DataType.String;
            argumentFormat = "time";
        }
#endif
        // TimeSpan is treated as the underlying type unless overridden:
        // else if (argumentType == typeof(TimeSpan) || argumentType == typeof(TimeSpan?))
        // {
        //     argumentOpenApiType = DataType.String";
        // }
        // Statement will pass if either object[] or IEnumerable<object>:
        else if (typeof(IEnumerable).IsAssignableFrom(argumentType))
        {
            argumentOpenApiType = DataType.Array;
        }
        else
        {
            argumentOpenApiType = DataType.Object;
        }
    }

    /// <summary>
    /// Ensure that an <see cref="Optional{T}"/> type which had been removed from the repo by mapping it to an object
    /// reference, then has its generic type included in the schema repository.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    private class OptionalDocumentFilter<T> : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            if (!context.SchemaRepository.Schemas.ContainsKey(typeof(T).Name))
                context.SchemaGenerator.GenerateSchema(typeof(T), context.SchemaRepository);
        }
    }

    /// <summary>
    /// Ensures that <see cref="Optional{T}"/> is correctly displayed in the API Schema in Swagger.
    /// </summary>
    /// <param name="options">Swagger options you get access to when calling `services.AddSwaggerGen(options => {});`.</param>
    /// <param name="assemblies">
    /// Pass in any assemblies that contain your controllers or DTOs to ensure that <see cref="Optional{T}"/> is
    /// correctly mapped.
    /// </param>
    public static SwaggerGenOptions FixOptionalMappings(this SwaggerGenOptions options, params Assembly[] assemblies)
    {
        if (!assemblies.Any())
            throw new ArgumentException("Expected at least one assembly.", nameof(assemblies));
        
        foreach (Assembly assembly in assemblies)
            options.FixMappingsForUsedOptionalsInAssembly(assembly);

        return options;
    }
}
