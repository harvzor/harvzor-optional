using Harvzor.Optional;
using Harvzor.Optional.NewtonsoftJson;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen(options =>
    {
        options.MapType<Optional<string>>(() => new OpenApiSchema
        {
            Type = "string"
        });
    });

builder.Services
    .AddControllers()
    // Minimal APIs don't work with Newtonsoft:
    // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis?view=aspnetcore-7.0#configure-json-deserialization-options-for-body-binding
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.Converters.Add(new OptionalJsonConverter());
        options.SerializerSettings.ContractResolver = new OptionalShouldSerializeContractResolver();
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
    public string Post(Foo foo)
    {
        if (foo.OptionalString.IsDefined)
            return $"You're value is \"{foo.OptionalString.Value ?? "null"}\".";
        
        return "You sent nothing.";
    }
}

public record Foo
{
    public Optional<string?> OptionalString { get; set; }
}
