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
        // options.MapType<Optional<string>>(() => new OpenApiSchema
        // {
        //     Type = "string"
        // });
        
        // Ensures that the weird extra types are never actually added, and are just mapped to string.
        options.MapType(typeof(Optional<>), () => new OpenApiSchema { Type = "string" } );

        // Remaps from strings to correct actual values.
        options.SchemaFilter<OptionalSchemaFilter>();
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

app
    .MapGet("/", () => "Hello World!");

app
    .MapPost("/", (Foo foo) => foo);

app.Run();

record Foo : Bar
{
    public Optional<Bar> Bar { get; set; }
}

record Bar
{
    public Optional<string?> OptionalNullableString { get; set; }
    
    // todo: I feel like `null` shouldn't be allowed?
    public Optional<string> OptionalString { get; set; }
    
    public string String { get; set; }
    
    public Optional<int?> OptionalNullableInt { get; set; }
    
    public Optional<int> OptionalInt { get; set; }
    
    public int Int { get; set; }
    
    //
    // public Optional<DateTime?> OptionalNullableDateTime { get; set; }
    //
    // public Optional<DateTime> OptionalDateTime { get; set; }
}

/// <summary>
/// Ensures that <see cref="Optional{T}"/> is correctly displayed in teh API Schema in Swagger.
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
        
            schema.Type = genericPartType.Type;
            schema.Properties.Clear();

            return;
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

                    property.Value.Type = propertyGenericPartType.Type;
                    property.Value.Properties.Clear();
                    property.Value.Reference = null;
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
