using System.Reflection;
using System.Text.Json.Serialization.Metadata;
using Harvzor.Optional;
using Harvzor.Optional.Swashbuckle;
using Harvzor.Optional.SystemTextJson;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen(options =>
    {
        options.MapType<Optional<Version>>(() => new OpenApiSchema()
        {
            Type = "string"
        });
        
        options.FixOptionalMappings(Assembly.GetExecutingAssembly());
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
    
    // [HttpPost]
    // public Optional<Foo> Post(Optional<Foo> foo)
    // {
    //     return foo;
    // }
    
    [HttpPost]
    public Foo Post(Foo foo)
    {
        return foo;
    }
}

public record Foo : Bar
{
    // public Optional<Bar> OptionalBar { get; set; }
    
    // public Bar Bar { get; set; }
}

public record Bar
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
    // public int Int { get; set; }
    //
    // public Optional<DateTime?> OptionalNullableDateTime { get; set; }
    //
    // public Optional<DateTime> OptionalDateTime { get; set; }
    //
    // public DateTime? NullableDateTime { get; set; }
    //
    // public DateTime DateTime { get; set; }
    //
    // public Optional<Version> OptionalVersion { get; set; }
    
    // public Optional<int[]> OptionalIntArray { get; set; }
    //
    // public int[] IntArray { get; set; } = { };
    
    public Optional<int[][]> OptionalIntArrayArray { get; set; }

    public int[][] IntArrayArray { get; set; }
}
