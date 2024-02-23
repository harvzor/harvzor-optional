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
