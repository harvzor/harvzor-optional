[![Test](https://github.com/harvzor/harvzor-optional/actions/workflows/test.yml/badge.svg)](https://github.com/harvzor/harvzor-optional/actions/workflows/test.yml) [![codecov](https://codecov.io/gh/harvzor/harvzor-optional/branch/master/graph/badge.svg?token=RT3EH7YLAA)](https://codecov.io/gh/harvzor/harvzor-optional)

# Harvzor.Optional

| NuGet Package                   | Version                                                                                                                                     | Purpose                                                                       |
|:--------------------------------|:--------------------------------------------------------------------------------------------------------------------------------------------|-------------------------------------------------------------------------------|
| Harvzor.Optional                | [![NuGet](https://img.shields.io/nuget/v/Harvzor.Optional)](https://www.nuget.org/packages/Harvzor.Optional/)                               | Contains core implementation of the `Optional<T>` type.                       |
| Harvzor.Optional.SystemTextJson | [![NuGet](https://img.shields.io/nuget/v/Harvzor.Optional.SystemTextJson)](https://www.nuget.org/packages/Harvzor.Optional.SystemTextJson/) | Enables JSON conversion for the `Optional<T>` type using System.Text.Json.    |
| Harvzor.Optional.NewtonsoftJson | [![NuGet](https://img.shields.io/nuget/v/Harvzor.Optional.NewtonsoftJson)](https://www.nuget.org/packages/Harvzor.Optional.NewtonsoftJson/) | Enables JSON conversion for the `Optional<T>` type using Newtonsoft Json.NET. |
| Harvzor.Optional.Swashbuckle    | [![NuGet](https://img.shields.io/nuget/v/Harvzor.Optional.Swashbuckle)](https://www.nuget.org/packages/Harvzor.Optional.Swashbuckle/)       | Helps with type mappings in `Swashbuckle.AspNetCore` Swagger.                 |

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

Install:

```
> dotnet add package Harvzor.Optional
```

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
using System.Text.Json.Serialization.Metadata;
using Harvzor.Optional;

JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions();
jsonSerializerOptions.Converters.Add(new Harvzor.Optional.SystemTextJson.OptionalJsonConverter());
jsonSerializerOptions.TypeInfoResolver = new DefaultJsonTypeInfoResolver
{
    Modifiers = { OptionalTypeInfoResolverModifiers.IgnoreUndefinedOptionals }
};

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

#### My APIs use System.Text.Json

First install:

```
> dotnet add package Harvzor.Optional.SystemTextJson
```

[There doesn't appear to be a way to globally change the default settings in System.Text.Json](https://github.com/dotnet/runtime/issues/31094), this means that minimal APIs and controller-based web APIs use different instances and settings of STJ.

So when you're trying to tell your APIs to use your specific converters, you need to make sure you set it up correctly:

```csharp
using Harvzor.Optional.SystemTextJson;

// IServiceCollection services; // Comes from your WebApplicationBuilder in Program.cs.

// If you're using controller-based web APIs:
services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
{
    options.JsonSerializerOptions.Converters.Add(new OptionalJsonConverter());
    options.JsonSerializerOptions.TypeInfoResolver = new DefaultJsonTypeInfoResolver
    {
        Modifiers = { OptionalTypeInfoResolverModifiers.IgnoreUndefinedOptionals }
    };
});

// Alternatively:
services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new OptionalJsonConverter());
        options.JsonSerializerOptions.TypeInfoResolver = new DefaultJsonTypeInfoResolver
        {
            Modifiers = { OptionalTypeInfoResolverModifiers.IgnoreUndefinedOptionals }
        };
    });

// If you're using minimal APIs:
services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new OptionalJsonConverter());
    options.SerializerOptions.TypeInfoResolver = new DefaultJsonTypeInfoResolver
    {
        Modifiers = { OptionalTypeInfoResolverModifiers.IgnoreUndefinedOptionals }
    };
});

// Alternatively:
services.ConfigureHttpJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new OptionalJsonConverter());
    options.JsonSerializerOptions.TypeInfoResolver = new DefaultJsonTypeInfoResolver
    {
        Modifiers = { OptionalTypeInfoResolverModifiers.IgnoreUndefinedOptionals }
    };
});
```

This solution was stolen from https://stackoverflow.com/questions/74889635/how-to-configure-json-name-case-policy-while-using-minimalapi/74889769#74889769.

#### My APIs use Newtonsoft.Json

First, install:

```
> dotnet add package Harvzor.Optional.NewtonsoftJson
```

```csharp
using Harvzor.Optional.NewtonsoftJson;

services
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.Converters.Add(new OptionalJsonConverter());
        options.SerializerSettings.ContractResolver = new IgnoreUndefinedOptionalsContractResolver();
    });
```

### Swagger support

#### Harvzor.Optional.Swashbuckle

> **Warning**
> This package is experimental.

Swashbuckle SwaggerGen doesn't know how to handle `Optional<T>` and will attempt to generate complicated objects to express all the properties, for a simple class like:

```csharp
public class Foo
{
    public Optional<int> OptionalInt { get; set; }
}
```

This ends up being generated like:

![broken-swagger-docs.png](/.github/docs/broken-swagger-docs.png)

Instead we want SwaggerGen to treat `Optional<T>` as the generic type `T`. To handle doing this, first install:

```
> dotnet add package Harvzor.Optional.NewtonsoftJson
```

Then add this magic line of code:

```csharp
using Harvzor.Optional.Swashbuckle;

services
    .AddSwaggerGen(options =>
    {
        // The assembly you pass in should include your controllers and perhaps even your DTOs.
        options.FixOptionalMappings(Assembly.GetExecutingAssembly());
    });
```

This results in the correct OpenAPI spec:

![fixed-swagger-docs.png](/.github/docs/fixed-swagger-docs.png)

This will:

- ensure that all basic types like `Optional<string>` are mapped to the `string` type in the OpenAPI schema
- try to treat complex objects such as `Optional<MyType>` as the underlying generic type `MyType`

This doesn't work in all cases though, for example, with `Optional<Version>`, we want it to be treated as a `string` type and not as a `Version`, so this must be added:

```csharp
using Harvzor.Optional.Swashbuckle;

// Add your custom mappings first:
options.MapType<Optional<Version>>(() => new OpenApiSchema()
{
    Type = "string"
});

options.FixOptionalMappings(Assembly.GetExecutingAssembly());
```

Alternatively, if you don't want to call `FixOptionalMappings(params Assembly[] assemblies)` which automagically finds any references to `Optional<T>` in your assembly, you can just directly feed it `Optional<T>` types that you know are used in your controllers:

```csharp
using Harvzor.Optional.Swashbuckle;

options
    .FixOptionalMappingForType<Optional<Foo>>()
    .FixOptionalMappingForType<Optional<Bar>>()
    .FixOptionalMappingForType<Optional<int>>();
```

##### Known caveats

- `FixOptionalMappings(params Assembly[] assemblies)` does not work with minimal APIs as it searches for `Optional<T>` references on parameters and properties of any classes that implement controller methods, and then maps those `Optional<T>` types to their generic type `T`
- `Optional<T>` doesn't work with query parameters
  - You can't have the following:
    ```csharp
    [HttpGet]
    public string Get([FromQuery] Optional<string> foo)
    {
        return foo;
    }
    ```
  - This is because of the following issue: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/2226

##### Improvements

This package could be improved if these issues are ever resolved:

- https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/1810
- https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/2359

#### Manual Swagger support

If you're using `Swashbuckle.AspNetCore.SwaggerGen` but don't want to use `Harvzor.Optional.Swashbuckle`, you can also manually tell it how your types should look. Here are some basic types mapped:

```csharp
services.AddSwaggerGen(options =>
{ 
    options.MapType<Optional<string>>(() => new OpenApiSchema
    {
        Type = "string"
    });
    
    options.MapType<Optional<int>>(() => new OpenApiSchema
    {
        Type = "integer",
        Format = "int32"
    });
    
    options.MapType<Optional<float>>(() => new OpenApiSchema
    {
        Type = "number",
        Format = "float"
    });
    
    options.MapType<Optional<double>>(() => new OpenApiSchema
    {
        Type = "number",
        Format = "double"
    });
    
    options.MapType<Optional<bool>>(() => new OpenApiSchema
    {
        Type = "boolean"
    });
    
    options.MapType<Optional<DateTime>>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "date-time"
    });
    
    options.MapType<Optional<IEnumerable<int>>>(() => new OpenApiSchema
    {
        Type = "array",
        Items = new OpenApiSchema
        {
            Type = "integer",
            Format = "int32"
        }
    });
    
    // There's way more types you can map, you should check which types your application uses and map those.
});
```

You can see what basic types are available here: https://swagger.io/docs/specification/data-models/data-types/

However, handling custom objects such as `Optional<MyObject>` is quite complicated and not recommended, however, here's how to do it anyway:

```csharp
// Rewrite the mapping so it's an object reference:
options.MapType<Optional<MyObject>>(() => new OpenApiSchema
{
    Type = "object",
    Format = format,
    Reference = new OpenApiReference
    {
        Id = nameof(MyObject),
        Type = ReferenceType.Schema,
    }
});

// Now add `MyObject` to the schema repisitory so the mapping actually points somewhere:
options.DocumentFilter<GenerateSchemaFor<MyObject>>();

private class GenerateSchemaFor<T> : IDocumentFilter where T : class
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
          context.SchemaGenerator.GenerateSchema(typeof(T), context.SchemaRepository);
    }
}
```

## Use case: JSON Merge PATCH

APIs that want to use [JSON Merge PATCH](https://datatracker.ietf.org/doc/html/rfc7386) (note that this is not [JSON Patch](https://datatracker.ietf.org/doc/html/rfc6902/)) need to have a way to distinguish between PATCH data not being sent (it being undefined) and a value being explicitly set to the default value. `Harvzor.Optional` provides this functionality.

Let's start with the following document stored in a database:

```json
{
  "title": "My Title",
  "dueDate": "2025-01-01T00:00:00Z"
}
```

A client may want to update either the `Title` or the `DueDate`, in this example they're updating just the title:

```json
{
  "title": "Do the thing"
}
```

The result should be:

```json
{
  "title": "Do the thing",
  "dueDate": "2025-01-01T00:00:00Z"
}
```

In our code we can represent the PATCH object with this DTO class:


```csharp
public class ToDoItemPatchDto
{
    public Optional<string> Title { get; set; }
    
    public Optional<DateTimeOffset?> DueDate { get; set; }
}
```

In our controller method, we can then update only the properties which were explicitly set and not make changes to the rest:

```csharp
[HttpPatch("{id:int}")]
[Consumes("application/merge-patch+json")]
public IActionResult PatchToDoItem([FromRoute] int id, [FromBody] ToDoItemPatchDto dto)
{
    var toDoItem = _toDoItemRepository.Find(id);
    
    if (toDoItem == null)
        return NotFound();

    if (dto.Title.IsDefined)
        toDoItem.Title = patch.Title.Value;
    
    if (dto.DueDate.IsDefined)
        toDoItem.DueDate = patch.DueDate.Value;

    toDoItem = _toDoItemRepository.Update(customer);
    
    return Ok(toDoItem.MapToDto());
}
```

There are of course many other packages which provide similar functionality, but I think `Harvzor.Optional` provides the cleanest and most versatile syntax. You can look at other solutions here: https://github.com/harvzor/undefined-and-null-in-dotnet/

## Releasing

### GitLab CI

1. Create a new release with a semver release name

### Manual release

In case the CI doesn't work:

1. Get an API key from https://www.nuget.org/account/apikeys
2. Run with Docker:
    ```
    docker-compose build --build-arg version="{version}" push-nuget
    docker-compose run --rm push-nuget --api-key {key}
    ```

## Further reading

- https://stackoverflow.com/questions/63418549/custom-json-serializer-for-optional-property-with-system-text-json
- https://stackoverflow.com/questions/12522000/optionally-serialize-a-property-based-on-its-runtime-value
- Optional in Swagger definition and how to handle generic types: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/2359
  - ISchemaGenerator: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/2333#issuecomment-1035695675
