using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    /// <param name="types">Should be of type <see cref="Optional{T}"/>.</param>
    private static void RemapOptionalObject(this SwaggerGenOptions options, params Type[] types)
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

        foreach (Type type in types)
        {
            Type argumentType = type.GetGenericArguments().First();
            
            try
            {
                // Mapping this will strangely mean that Optional<T> is not picked up in the SchemaFilter, though other
                // types like Optional<int> are (even if they've been mapped!).
                options.MapType(type, () => new OpenApiSchema
                {
                    Type = "object",
                    // Seems that Swagger doesn't mark references as nullable ever?
                    // Nullable = argumentType.IsGenericType
                    //     && argumentType.GetGenericTypeDefinition() == typeof(Nullable<>),
                    Reference = new OpenApiReference
                    {
                        Id = argumentType.Name,
                        Type = ReferenceType.Schema,
                    }
                });
            }
            catch (Exception)
            {
                // todo: maybe catch can be removed if options.SchemaGeneratorOptions.CustomTypeMappings is checked?
                // probably can't generate schemas that way though as there's no access to schema generator

                // If the same type is attempted to be mapped twice, the second attempt will be caught here.
                // There doesn't appear to be a way to check which schemas have already been mapped.
            }

            // This should do the same as `options.DocumentFilter<OptionalDocumentFilter<T>>();`
            Type genericType = CreateGenericOptionalDocumentFilter(argumentType);
            InvokeDocumentFilter(genericType);
        }
    }
    
    private static void FixMappingsForUsedOptionalsInAssembly(this SwaggerGenOptions options, Assembly assembly)
    {
        Type openGenericOptionalType = typeof(Optional<>);
        
        // Recursively find all the Optional<T> properties on the given type.
        IEnumerable<Type> WalkPropertiesAndFindOptionalProperties(Type type)
        {
            var nonOptionalType = type.IsGenericType && type.GetGenericTypeDefinition() == openGenericOptionalType
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
            .Where(type => type.IsSubclassOf(typeof(Microsoft.AspNetCore.Mvc.ControllerBase)))
            .SelectMany(assemblyType =>
            {
                // Find parameters on controller methods like `Foo foo` on `public Foo Post(Foo foo)`:
                Type[] parameters = assemblyType
                    // This doesn't work with a minimal API like:
                    // `app.MapPost("/", (Optional<Foo> foo) => foo);`
                    // as it doesn't look like Optional<Foo> is defined in the assembly?
                    .GetMethods()
                    .Where(m => m.DeclaringType == assemblyType)
                    .SelectMany(method => method
                        .GetParameters()
                        .Select(x => x.ParameterType)
                        // todo: could also be IActionResult, consumer of this should be able to specify what types there are
                        .Concat(new []{method.ReturnType })
                    )
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
            options.MapType(typeUsingOptional);
    }

    private static void MapType(this SwaggerGenOptions options, Type typeUsingOptional)
    {
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
                // todo: create example project showing Nullable doesn't seem to work
                Nullable = argumentType.IsReferenceOrNullableType() // Should catch `string` and `int?`.
                    // || (
                    //     argumentType.IsGenericType
                    //     && argumentType.GetGenericTypeDefinition() == typeof(Nullable<>)
                    // ),
            });
        }
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
        else if (argumentType == typeof(string))
        {
            argumentOpenApiType = "string";
        }
        else if (argumentType == typeof(bool) || argumentType == typeof(bool?))
        {
            argumentOpenApiType = "boolean";
        }
        // todo: validate DateTime works like this
        else if (argumentType == typeof(DateTime) || argumentType == typeof(DateTime?))
        {
            argumentOpenApiType = "string";
            argumentFormat = "date-time";
        }
        // todo: validate TimeSpan works like this
        else if (argumentType == typeof(TimeSpan) || argumentType == typeof(TimeSpan?))
        {
            argumentOpenApiType = "string";
        }
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
    private class OptionalDocumentFilter<T> : IDocumentFilter where T : class
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
    /// <param name="options"></param>
    /// <param name="assemblies">
    /// Pass in any assemblies that contain your controllers or DTOs to ensure that <see cref="Optional{T}"/> is
    /// correctly mapped.
    /// </param>
    public static void FixOptionalMappings(this SwaggerGenOptions options, params Assembly[] assemblies)
    {
        foreach (Assembly assembly in assemblies)
            options.FixMappingsForUsedOptionalsInAssembly(assembly);
    }
}
