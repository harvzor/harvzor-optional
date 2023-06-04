## Harvzor.Optional.SystemTextJson

### Use in an API

To use it in your controller models, simply register in your startup:

```csharp
services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new Harvzor.Optional.SystemTextJson.OptionalJsonConverter());
    });
```

#### Swagger

If you're using Swashbuckle Swagger, you'll also need to tell it how your types should look:

```csharp
services.AddSwaggerGen(options =>
{ 
    options.MapType<Optional<string>>(() => new OpenApiSchema
    {
        Type = "string"
    });
    
    options.MapType<Optional<bool>>(() => new OpenApiSchema
    {
        Type = "boolean"
    });
});
```

You can see what basic types are available here: https://swagger.io/docs/specification/data-models/data-types/
