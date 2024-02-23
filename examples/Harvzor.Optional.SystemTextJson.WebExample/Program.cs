using System.Reflection;
using System.Text.Json.Serialization.Metadata;
using Harvzor.Optional;
using Harvzor.Optional.Swashbuckle;
using Harvzor.Optional.SystemTextJson;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddSwaggerGen(options =>
    {
        options.MapType<Optional<Version>>(() => new OpenApiSchema
        {
            Type = "string"
        });
        
        options.FixOptionalMappings(Assembly.GetExecutingAssembly());
        // options
        //     .FixOptionalMappingForType<Optional<Foo>>()
        //     .FixOptionalMappingForType<Optional<Bar>>()
        //     .FixOptionalMappingForType<Optional<int>>();
    });

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new OptionalJsonConverter());
        options.JsonSerializerOptions.TypeInfoResolver = new DefaultJsonTypeInfoResolver
        {
            Modifiers = { OptionalTypeInfoResolverModifiers.IgnoreUndefinedOptionals }
        };
    });

var app = builder.Build();

app.UseRouting();
app.MapDefaultControllerRoute();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();

[Route("/")]
[ApiController]
public class IndexController : Controller
{
    [HttpGet]
    public string Get()
    {
        return "Hello World!";
    }
    
    [HttpPost]
    public Foo Post(Foo foo)
    {
        Console.WriteLine(
            foo.OptionalString.IsDefined
                ? $"You sent: {(foo.OptionalString.Value == null ? "null" : $"\"{foo.OptionalString.Value}\"")}"
                : "You sent nothing!"
        );

        return foo;
    }
}

public class Foo
{
    public Optional<string?> OptionalString { get; set; }
}

// todo: test https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/v6.5.0/test/Swashbuckle.AspNetCore.IntegrationTests/DocumentProviderTests.cs
// https://stackoverflow.com/questions/62996494/unit-testing-that-the-swagger-doc-is-correct-without-starting-a-server