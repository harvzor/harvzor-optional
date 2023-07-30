using System.Reflection;
using System.Text.Json.Serialization.Metadata;
using Harvzor.Optional;
using Harvzor.Optional.SystemTextJson;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen(options =>
    {
        options.FixOptional();

        // // Ensures that the weird extra types are never actually added, and are just mapped to string.
        // options.MapType(typeof(Optional<>), () => new OpenApiSchema { Type = "string" } );
        //
        // // Remaps from strings to correct actual values.
        // options.SchemaFilter<OptionalSchemaFilter>();
    });

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new OptionalJsonConverter());
    options.SerializerOptions.TypeInfoResolver = new DefaultJsonTypeInfoResolver
    {
        Modifiers = { OptionalTypeInfoResolverModifiers.IgnoreUndefinedOptionals }
    };
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app
//     .MapGet("/", () => "Hello World!");

app
    .MapPost("/", (Optional<Foo> foo) => foo);

app.Run();

record Foo : Bar
{
    // public Optional<Bar> OptionalBar { get; set; }
    
    public Bar Bar { get; set; }
}

record Bar
{
    // public Optional<string?> OptionalNullableString { get; set; }
    //
    // // todo: I feel like `null` shouldn't be allowed?
    // public Optional<string> OptionalString { get; set; }
    //
    // public string String { get; set; }
    //
    // public Optional<int?> OptionalNullableInt { get; set; }
    //
    // public Optional<int> OptionalInt { get; set; }
    //
    // public int? NullableInt { get; set; }
    //
    public int Int { get; set; }
    
    //
    // public Optional<DateTime?> OptionalNullableDateTime { get; set; }
    //
    // public Optional<DateTime> OptionalDateTime { get; set; }
}

/// <summary>
/// Ensures that <see cref="Optional{T}"/> is correctly displayed in the API Schema in Swagger.
/// </summary>
/// <remarks>
/// https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/2359#issuecomment-1114008607
/// </remarks>
public class OptionalSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.IsGenericType && context.Type.GetGenericTypeDefinition() == typeof(Optional<>))
        {
            Type itemType = context.Type.GetGenericArguments()[0];
            OpenApiSchema? genericPartType = context.SchemaGenerator.GenerateSchema(itemType, context.SchemaRepository);

            // If `Type` is null, it's property a user defined class.
            if (genericPartType.Type == null)
            {
                // schema.Reference = new OpenApiReference()
                // {
                //     Id = "#/components/schemas/" + itemType.Name,
                // };
                // schema.Type = null;
                // schema.Properties.Clear();
            }
            else
            {
                schema.Type = genericPartType.Type;
                schema.Properties.Clear();
                
                return;
            }
        }

        if (context.Type.GetProperties().Any(p =>
                p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(Optional<>)))
        {
            foreach (var property in schema.Properties)
            {
                PropertyInfo? propertyInfo = context.Type.GetProperties()
                    .FirstOrDefault(x => x.Name.Equals(property.Key, StringComparison.OrdinalIgnoreCase));

                if (propertyInfo != null
                    && propertyInfo.PropertyType.IsGenericType
                    && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Optional<>))
                {
                    // context.SchemaRepository.Schemas.Remove(property.Value.Reference.Id);
                    
                    Type propertyType = propertyInfo.PropertyType.GetGenericArguments()[0];
                    OpenApiSchema? propertyGenericPartType = context.SchemaGenerator.GenerateSchema(propertyType, context.SchemaRepository);

                    // If `Type` is null, it's property a user defined class.
                    // The property.Value.Reference should then be defined.
                    if (propertyGenericPartType.Type == null)
                    {
                        // It's set to string if `options.MapType(typeof(Optional<>), () => new OpenApiSchema { Type = "string" } );` is used.
                        property.Value.Type = null;
                        property.Value.Reference = propertyGenericPartType.Reference;
                    }
                    else
                    {
                        property.Value.Type = propertyGenericPartType.Type;
                        property.Value.Properties.Clear();
                        property.Value.Reference = null;
                    }
                }
            }
        }
    }
    
    // public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    // {
    //     if (context.Type.IsGenericType && context.Type.GetGenericTypeDefinition() == typeof(Optional<>))
    //     {
    //         var argumentType = context.Type.GetGenericArguments().First();
    //         var argumentSchema = context.SchemaGenerator.GenerateSchema(argumentType, context.SchemaRepository);
    //         var baseSchemaName = $"{argumentType.Name}Foo";
    //         var baseSchema = new OpenApiSchema()
    //         {
    //             Required = new SortedSet<string>() { "type" },
    //             Type = "object",
    //             Properties = new Dictionary<string, OpenApiSchema>
    //             {
    //                 { "type", argumentSchema }
    //             }
    //         };
    //         context.SchemaRepository.AddDefinition(baseSchemaName, baseSchema);
    //         schema.Type = "string";
    //         schema.Reference = new OpenApiReference { Id = $"{baseSchemaName}", Type = ReferenceType.Schema };
    //     }
    // }
}

/// <summary>
/// Ensures that <see cref="Optional{T}"/> is correctly displayed in the API Schema in Swagger.
/// </summary>
public static class Solution2
{
    private static void RemapUserClass<T>(this SwaggerGenOptions options) where T : class
    {
        try
        {
            // Mapping this will strangely mean that Optional<T> is not picked up in the SchemaFilter, though other types
            // like Optional<int> are (even if they've been mapped!).
            options.MapType<Optional<T>>(() => new OpenApiSchema
            {
                Type = "object",
                Reference = new OpenApiReference()
                {
                    Id = typeof(T).Name,
                    Type = ReferenceType.Schema,
                }
            });
        }
        catch (Exception)
        {
            // todo: find a nicer way to deal with types being added multiple times (somehow look through schema repo?)
        }

        options.DocumentFilter<OptionalDocumentFilter<T>>();
    }
    
    /// <summary>
    /// Without remapping, system objects like `"#/components/schemas/Type` will be included as Swagger will try mapping
    /// <see cref="Optional{T}"/> properties.
    /// </summary>
    private static void EnsureBadObjectsDontShow(this SwaggerGenOptions options)
    {
        options.MapType<Optional<string>>(() => new OpenApiSchema
        {
            Type = "string",
            // Seems that string is nullable even when the context doesn't say it's nullable...
            Nullable = true
        });
        
        // So there's no need for this as it's the same as `string`:
        // options.MapType<Optional<string?>>(() => new OpenApiSchema
        // {
        //     Type = "string"
        //     Nullable = true,
        // });
        
        options.MapType<Optional<int>>(() => new OpenApiSchema
        {
            Type = "integer"
        });
        
        options.MapType<Optional<int?>>(() => new OpenApiSchema
        {
            Type = "integer",
            Nullable = true
        });

        options.RemapUserClass<Foo>();
        options.RemapUserClass<Bar>();
        options.RemapUserClass<Bar>();
    }
    
    // private class OptionalObjectsSchemaFilter : ISchemaFilter
    // {
    //     public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    //     {
    //         if (context.Type.IsGenericType && context.Type.GetGenericTypeDefinition() == typeof(Optional<>))
    //         {
    //             Type itemType = context.Type.GetGenericArguments()[0];
    //
    //             // Ensure this is a custom defined type and not a system type like System.String.
    //             if (itemType.Assembly == Assembly.GetExecutingAssembly())
    //             {
    //                 OpenApiSchema? genericPartType = context.SchemaGenerator.GenerateSchema(itemType, context.SchemaRepository);
    //                 context.SchemaRepository.Schemas.Add(itemType.Name, genericPartType);
    //             }
    //         }
    //     }
    // }
    
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
    
    public static void FixOptional(this SwaggerGenOptions options)
    {
        options.EnsureBadObjectsDontShow();
        
        // options.SchemaFilter<OptionalObjectsSchemaFilter>();
    }
}
