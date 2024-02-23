using System.Reflection;
using System.Text.Json.Serialization.Metadata;
using Harvzor.Optional;
using Harvzor.Optional.Swashbuckle;
using Harvzor.Optional.SystemTextJson;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddSwaggerGen(options =>
    {
        // Map types manually without Harvzor.Optional.Swashbuckle:
        // options.MapType<Optional<string?>>(() => new OpenApiSchema
        // {
        //     Type = "string"
        // });
        
        // Auto fixes mappings:
        options.FixOptionalMappings(Assembly.GetExecutingAssembly());
        
        // Alternatively, specify specific types that should be fixed:
        // options
        //     .FixOptionalMappingForType<Optional<string>>();
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
