using System.Reflection;
using Harvzor.Optional;
using Harvzor.Optional.NewtonsoftJson;
using Microsoft.AspNetCore.Mvc;
using Harvzor.Optional.Swashbuckle;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddSwaggerGen(options =>
    {
        options.FixOptionalMappings(Assembly.GetExecutingAssembly());
    })
    .AddSwaggerGenNewtonsoftSupport();

builder.Services
    .AddControllers()
    // Minimal APIs don't work with Newtonsoft:
    // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis?view=aspnetcore-7.0#configure-json-deserialization-options-for-body-binding
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.Converters.Add(new OptionalJsonConverter());
        options.SerializerSettings.ContractResolver = new IgnoreUndefinedOptionalsContractResolver();
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
    public Optional<Foo> Post(Optional<Foo> foo)
    {
        return foo;
    }
}

public record Foo : Bar
{
    public Optional<Bar?> OptionalNullableBar { get; set; }
    
    public Optional<Bar> OptionalBar { get; set; }

    public Bar? NullableBar { get; set; }
    
    public Bar Bar { get; set; }
}

public record Bar
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
}
