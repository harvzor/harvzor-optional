[![Build](https://github.com/harvzor/harvzor-optional/actions/workflows/build.yml/badge.svg)](https://github.com/harvzor/harvzor-optional/actions/workflows/build.yml)

# Harvzor.Optional

| NuGet Package                   | Version                                                                                                                                     |
|:--------------------------------|:--------------------------------------------------------------------------------------------------------------------------------------------|
| Harvzor.Optional                | [![NuGet](https://img.shields.io/nuget/v/Harvzor.Optional)](https://www.nuget.org/packages/Harvzor.Optional/)                               |
| Harvzor.Optional.SystemTextJson | [![NuGet](https://img.shields.io/nuget/v/Harvzor.Optional.SystemTextJson)](https://www.nuget.org/packages/Harvzor.Optional.SystemTextJson/) |
| Harvzor.Optional.NewtonsoftJson | [![NuGet](https://img.shields.io/nuget/v/Harvzor.Optional.NewtonsoftJson)](https://www.nuget.org/packages/Harvzor.Optional.NewtonsoftJson/) |

## The problem

```csharp
// The JSON would normally come from some external data source:
string json = "{}";

Foo foo = JsonSerializer.Deserialize<Foo>(json);

// This will print an empty string because C# will hydrate the model with the default value.
// There's no way to check if the model used a default value.
Console.WriteLine(foo.MyProperty); // ""    

public class Foo
{
    public string MyProperty { get; set; }
}
```

### So just use a nullable value? (`string?`)

In the above example, you could make the string nullable (with `string?`), but now you're explicitly saying that null is value you want to accept (which you'll have to handle in your code).

What if you want to allow a client to explicitly set null (`"{ "myProperty": null }"`), and want to handle this in your code, while also knowing if the client didn't send *anything*?

## The solution: Optional&lt;T&gt;

### Basic example

You can use `Optional<T>` to know if a property or variable has been explicitly instantiated:

```csharp
using Harvzor.Optional;

Foo foo = new Foo
{
    DefinedProperty = "Bar"
};

Console.WriteLine(foo.DefinedProperty.IsDefined); // True
Console.WriteLine(foo.UndefinedProperty.IsDefined); // False

// Now I can check if a value was explicitly instantiated:
if (foo.DefinedProperty.IsDefined) // True
    Console.WriteLine(foo.DefinedProperty.Value); // "Bar" 
    
if (foo.UndefinedProperty.IsDefined) // False
    Console.WriteLine(foo.UndefinedProperty.Value); // Won't print as the value wasn't explicitly instantiated.

Console.WriteLine(foo.UndefinedProperty.Value); // Will print the default value.

public class Foo
{
    public Optional<string> DefinedProperty { get; set; }
    public Optional<string> UndefinedProperty { get; set; }
}
```

### Use it with JSON

This example uses `System.Text.Json`:

```csharp
using System.Text.Json;
using Harvzor.Optional;

JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions();
jsonSerializerOptions.Converters.Add(new Harvzor.Optional.SystemTextJson.OptionalJsonConverter());

// The JSON would normally come from some external data source:
string json = "{\"DefinedProperty\":\"Bar\"}";

Foo foo = JsonSerializer.Deserialize<Foo>(json, jsonSerializerOptions)!;

Console.WriteLine(foo.DefinedProperty.IsDefined); // True
Console.WriteLine(foo.UndefinedProperty.IsDefined); // False

// Now I can check if a value was defined before I try using it:
if (foo.DefinedProperty.IsDefined) // True
    Console.WriteLine(foo.DefinedProperty.Value); // "Bar" 
    
if (foo.UndefinedProperty.IsDefined) // False
    Console.WriteLine(foo.UndefinedProperty.Value); // Won't print as the value wasn't defined.

public class Foo
{
    public Optional<string> DefinedProperty { get; set; }
    public Optional<string> UndefinedProperty { get; set; }
}
```

### Use in an API

To use it in your controller models, simply register in your startup:

#### Harvzor.Optional.SystemTextJson

```csharp
services
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new Harvzor.Optional.SystemTextJson.OptionalJsonConverter());
    });
```

#### Harvzor.Optional.NewtonsoftJson

```csharp
services
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.Converters.Add(new Harvzor.Optional.NewtonsoftJson.OptionalJsonConverter());
    });
```

#### Swagger support

If you're using Swashbuckle Swagger, you'll also need to tell it how your types should look:

```csharp
services.AddSwaggerGen(options =>
{ 
    options.MapType<Optional<string>>(() => new OpenApiSchema
    {
        Type = "string"
    });
    
    options.MapType<Optional<int>>(() => new OpenApiSchema
    {
        Type = "integer"
    });
    
    options.MapType<Optional<float>>(() => new OpenApiSchema
    {
        Type = "number"
    });
    
    options.MapType<Optional<double>>(() => new OpenApiSchema
    {
        Type = "number"
    });
    
    options.MapType<Optional<bool>>(() => new OpenApiSchema
    {
        Type = "boolean"
    });
    
    // todo: array, object?
});
```

You can see what basic types are available here: https://swagger.io/docs/specification/data-models/data-types/

## Use case: JSON Merge PATCH

... need docs ...

## Releasing

### GitLab CI

1. Create a new release with a semver release name

### Manual release

In case the CI doesn't work:

1. Get an API key from https://www.nuget.org/account/apikeys
2.
```
docker-compose build --build-arg version="{version}" push-nuget
docker-compose run --rm push-nuget --api-key {key}
```

## Further reading

- https://stackoverflow.com/questions/63418549/custom-json-serializer-for-optional-property-with-system-text-json
- https://stackoverflow.com/questions/12522000/optionally-serialize-a-property-based-on-its-runtime-value
