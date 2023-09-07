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
    public IActionResult Post([FromBody] Foo foo)
    {
        return Ok(new Optional<Foo>());
    }
}

public class Foo
{
    public Optional<string> String { get; set; }
    
    // public Optional<Bar?> OptionalBar { get; set; }
    
    // public Bar Bar { get; set; }
}

public class Bar
{
    public Optional<string?> OptionalNullableString { get; set; }
    
    // todo: I feel like `null` shouldn't be allowed?
    public Optional<string> OptionalString { get; set; }
    
    public string String { get; set; }
    
    public Optional<int?> OptionalNullableInt { get; set; }
    
    public Optional<int> OptionalInt { get; set; }
    
    public int? NullableInt { get; set; }
    
    public int Int { get; set; }
    
    public Optional<DateTime?> OptionalNullableDateTime { get; set; }
    
    public Optional<DateTime> OptionalDateTime { get; set; }
    
    public DateTime? NullableDateTime { get; set; }
    
    public DateTime DateTime { get; set; }
    
    public Optional<TimeSpan?> OptionalNullableTimeSpan { get; set; }
    
    public Optional<TimeSpan> OptionalTimeSpan { get; set; }
    
    public TimeSpan? NullableTimeSpan { get; set; }
    
    public TimeSpan TimeSpan { get; set; }
    
    public Optional<Version> OptionalVersion { get; set; }
    
    public Optional<int[]> OptionalIntArray { get; set; }
    
    public int[] IntArray { get; set; } = { };
    
    public Optional<int[][]> OptionalIntArrayArray { get; set; }
    
    public int[][] IntArrayArray { get; set; }
}

// todo: test https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/v6.5.0/test/Swashbuckle.AspNetCore.IntegrationTests/DocumentProviderTests.cs
// https://stackoverflow.com/questions/62996494/unit-testing-that-the-swagger-doc-is-correct-without-starting-a-server