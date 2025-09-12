using System.Text.Json.Serialization.Metadata;
using Harvzor.Optional;
using Harvzor.Optional.SystemTextJson;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options => {
    // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/customize-openapi?view=aspnetcore-9.0
    options.AddSchemaTransformer((schema, context, _) =>
    {
        if (context.JsonTypeInfo.Type == typeof(Optional<string?>))
        {
            schema.Type = "string";
            schema.Properties.Clear();
            schema.Annotations.Clear();
        }
        return Task.CompletedTask;
    });
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "My API V1");
    });
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
