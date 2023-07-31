﻿using System.Collections;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Harvzor.Optional.Swashbuckle;

public static class OptionalSwashbuckle
{
    private static void RemapOptionalObject(this SwaggerGenOptions options, params Type[] types)
    {
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
                // todo: find a nicer way to deal with types being added multiple times (somehow look through schema repo?)
            }

            // This should do the same as `options.DocumentFilter<OptionalDocumentFilter<T>>();`
            {
                Type? genericType;
                // Create OptionalDocumentFilter<T>.
                {
                    Type optionalDocumentFilterType = typeof(OptionalDocumentFilter<>);
                    genericType = optionalDocumentFilterType.MakeGenericType(argumentType);
                }

                // Invoke `options.DocumentFilter<OptionalDocumentFilter<T>>()` but with `genericType` instead of `<T>`;
                {
                    MethodInfo documentFilterMethod = typeof(SwaggerGenOptionsExtensions)
                        .GetMethod(nameof(SwaggerGenOptionsExtensions.DocumentFilter))!;

                    MethodInfo genericDocumentFilterMethod = documentFilterMethod.MakeGenericMethod(genericType);

                    genericDocumentFilterMethod.Invoke(null, new object?[] { options, Array.Empty<object>() });
                }
            }
        }
    }
    
    private static void FixMappingsForUsedOptionalsInAssembly(this SwaggerGenOptions options, Assembly assembly)
    {
        Type openGenericOptionalType = typeof(Optional<>);
        
        // Find any Optional<T>'s.
        IEnumerable<Type> typesUsingOptional = assembly.GetTypes()
            // todo: also check fields?
            .SelectMany(assemblyType =>
            {
                IEnumerable<Type> optionalProperties = assemblyType.GetProperties()
                    .Select(x => x.PropertyType)
                    .Where(propertyType => propertyType.IsGenericType
                                           && propertyType.GetGenericTypeDefinition() == openGenericOptionalType
                    );

                // todo: this doesn't work with a minimal API like:
                // app.MapPost("/", (Optional<Foo> foo) => foo);
                // as it doesn't look like Optional<Foo> is defined in the assembly?
                IEnumerable<Type> optionalParameters = assemblyType
                    .GetMethods( /*BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static*/)
                    .SelectMany(method => method.GetParameters().Select(x => x.ParameterType))
                    .Where(parameterType => parameterType.IsGenericType
                                            && parameterType.GetGenericTypeDefinition() == openGenericOptionalType
                    );

                return optionalProperties.Concat(optionalParameters);
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
                // todo: can return true, but doesn't do anything? worked if same code is used with Deprecated
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
            // todo: don't throw?
            throw new Exception("unexpected type");
        }
        
        GetTypeAndFormat(arrayType, out string arrayStringType, out string? arrayFormat);

        // If it's a multidimensional array...
        if (arrayStringType == "array")
        {
            return new OpenApiSchema
            {
                Type = "array",
                // todo: not sure if it should be nullable
                Nullable = true,
                Items = GetSchemaOfArray(arrayType)
            };
        }
        else
        {
            return new OpenApiSchema
            {
                Type = "array",
                // todo: not sure if it should be nullable
                Nullable = true,
                Items = new OpenApiSchema()
                {
                    Type = arrayStringType,
                    Format = arrayFormat
                }
            };
        }

        // // If it's a multidimensional array...
        // if (typeof(IEnumerable).IsAssignableFrom(argumentType))
        // {
        //     
        // }
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
        else if (argumentType == typeof(DateTime) || argumentType == typeof(DateTime?))
        {
            argumentOpenApiType = "string";
            argumentFormat = "date-time";
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
        // todo: provide docs on how to manage other types? or is it better to try to map simple types without this hack?
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

// /// <summary>
// /// Ensures that <see cref="Optional{T}"/> is correctly displayed in the API Schema in Swagger.
// /// </summary>
// /// <remarks>
// /// https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/2359#issuecomment-1114008607
// /// </remarks>
// public class OptionalSchemaFilter : ISchemaFilter
// {
//     public void Apply(OpenApiSchema schema, SchemaFilterContext context)
//     {
//         if (context.Type.IsGenericType && context.Type.GetGenericTypeDefinition() == typeof(Optional<>))
//         {
//             Type itemType = context.Type.GetGenericArguments()[0];
//             OpenApiSchema? genericPartType = context.SchemaGenerator.GenerateSchema(itemType, context.SchemaRepository);
//         
//             // If `Type` is null, it's property a user defined class.
//             if (genericPartType.Type == null)
//             {
//                 // schema.Reference = new OpenApiReference
//                 // {
//                 //     Id = "#/components/schemas/" + itemType.Name,
//                 // };
//                 // schema.Type = null;
//                 // schema.Properties.Clear();
//             }
//             else
//             {
//                 schema.Type = genericPartType.Type;
//                 schema.Properties.Clear();
//                 
//                 return;
//             }
//         }
//
//         if (context.Type.GetProperties().Any(p =>
//                 p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(Optional<>)))
//         {
//             foreach (var property in schema.Properties)
//             {
//                 PropertyInfo? propertyInfo = context.Type.GetProperties()
//                     .FirstOrDefault(x => x.Name.Equals(property.Key, StringComparison.OrdinalIgnoreCase));
//
//                 if (propertyInfo != null
//                     && propertyInfo.PropertyType.IsGenericType
//                     && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Optional<>))
//                 {
//                     // context.SchemaRepository.Schemas.Remove(property.Value.Reference.Id);
//                     
//                     Type propertyType = propertyInfo.PropertyType.GetGenericArguments()[0];
//                     OpenApiSchema? propertyGenericPartType = context.SchemaGenerator.GenerateSchema(propertyType, context.SchemaRepository);
//
//                     // If `Type` is null, it's property a user defined class.
//                     // The property.Value.Reference should then be defined.
//                     if (propertyGenericPartType.Type == null)
//                     {
//                         // // It's set to string if `options.MapType(typeof(Optional<>), () => new OpenApiSchema { Type = "string" } );` is used.
//                         property.Value.Type = null;
//                         property.Value.Reference = propertyGenericPartType.Reference;
//                     }
//                     else
//                     {
//                         property.Value.Type = propertyGenericPartType.Type;
//                         property.Value.Properties.Clear();
//                         property.Value.Reference = null;
//                     }
//                 }
//             }
//         }
//     }
// }
//
