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
    /// <param name="optionalTypes">Should be of type <see cref="Optional{T}"/>.</param>
    private static void RemapOptionalObject(this SwaggerGenOptions options, params Type[] optionalTypes)
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

        foreach (Type optionalType in optionalTypes)
        {
            Type genericType = optionalType.GetGenericArguments().First();

            bool isNullable = genericType.IsGenericType
                && genericType.GetGenericTypeDefinition() == typeof(Nullable<>);
            
            if (isNullable)
                genericType = genericType.GetGenericArguments().First();

            // Ensure the same type isn't mapped twice somehow otherwise `options.MapType` will throw.
            if (options.SchemaGeneratorOptions.CustomTypeMappings.Any(x => x.Key == optionalType))
                continue;
            
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
    }
    
    private static void FixMappingsForUsedOptionalsInAssembly(this SwaggerGenOptions options, Assembly assembly)
    {
        Type openGenericOptionalType = typeof(Optional<>);
        
        // Recursively find all the Optional<T> properties on the given type.
        IEnumerable<Type> WalkPropertiesAndFindOptionalProperties(Type type)
        {
            Type nonOptionalType = type.IsGenericType && type.GetGenericTypeDefinition() == openGenericOptionalType
                ? type.GetGenericArguments().First()
                : type;

            Type[] propertyTypes = nonOptionalType
                .GetProperties()
                .Select(property => property.PropertyType)
                // Prevent recursion loop.
                .Where(propertyType => propertyType != type)
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

    /// <inheritdoc cref="FixOptionalMappingForType"/>
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
        
        GetTypeAndFormat(argumentType, out string argumentOpenApiType, out string? argumentFormat);
        
        if (argumentOpenApiType == "object")
        {
            options.RemapOptionalObject(typeUsingOptional);
        }
        else if (argumentOpenApiType == "array")
        {
            options.MapType(typeUsingOptional, () => GetSchemaOfArray(argumentType));
        }
        // Normal types:
        else
        {
            options.MapType(typeUsingOptional, () => new OpenApiSchema
            {
                Type = argumentOpenApiType,
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
        
        GetTypeAndFormat(arrayType, out string arrayStringType, out string? arrayFormat);

        // If it's a multidimensional array...
        if (arrayStringType == "array")
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
            Items = new OpenApiSchema()
            {
                Type = arrayStringType,
                Format = arrayFormat,
                Nullable = arrayType.IsReferenceOrNullableType()
            }
        };
    }

    // todo: may want to ensure entire list from here is used https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/8f363f7359cb1cb8fa5de5195ec6d97aefaa16b3/test/Swashbuckle.AspNetCore.SwaggerGen.Test/SchemaGenerator/JsonSerializerSchemaGeneratorTests.cs#L35
    private static void GetTypeAndFormat(Type argumentType, out string argumentOpenApiType, out string? argumentFormat)
    {
        argumentFormat = null;

        if (argumentType == typeof(int) || argumentType == typeof(int?))
        {
            argumentOpenApiType = "integer";
            argumentFormat = "int32";
        }
        else if (argumentType == typeof(long) || argumentType == typeof(long?))
        {
            argumentOpenApiType = "integer";
            argumentFormat = "int64";
        }
        else if (argumentType == typeof(float) || argumentType == typeof(float?))
        {
            argumentOpenApiType = "number";
            argumentFormat = "float";
        }
        else if (argumentType == typeof(double) || argumentType == typeof(double?))
        {
            argumentOpenApiType = "number";
            argumentFormat = "double";
        }
        else if (argumentType == typeof(string) /* || argumentType == typeof(string?) */)
        {
            argumentOpenApiType = "string";
        }
        else if (argumentType == typeof(bool) || argumentType == typeof(bool?))
        {
            argumentOpenApiType = "boolean";
        }
        else if (argumentType == typeof(DateTime) || argumentType == typeof(DateTime?))
        {
            argumentOpenApiType = "string";
            argumentFormat = "date-time";
        }
        // TimeSpan is treated as the underlying type unless overridden:
        // else if (argumentType == typeof(TimeSpan) || argumentType == typeof(TimeSpan?))
        // {
        //     argumentOpenApiType = "string";
        // }
        // Not supported in netstandard:
        // else if (argumentType == typeof(DateOnly) || argumentType == typeof(DateOnly?))
        // {
        //     stringType = "string";
        //     format = "date";
        // }
        // Statement will pass if either object[] or IEnumerable<object>:
        else if (typeof(IEnumerable).IsAssignableFrom(argumentType))
        {
            argumentOpenApiType = "array";
        }
        else
        {
            argumentOpenApiType = "object";
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
