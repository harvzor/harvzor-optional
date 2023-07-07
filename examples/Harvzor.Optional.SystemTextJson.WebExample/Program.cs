using Harvzor.Optional;
using Microsoft.OpenApi.Models;
using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

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

builder.Services.Configure<JsonOptions>(o => o.SerializerOptions.Converters.Add(new Harvzor.Optional.SystemTextJson.OptionalJsonConverter()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app
    .MapGet("/", () => "Hello World!");

app
    .MapPost("/", (Foo foo) =>
    {
        if (foo.OptionalString.IsDefined)
            return $"You're value is \"{foo.OptionalString.Value ?? "null"}\".";
        
        return "You sent nothing.";
    });

app.Run();

record Foo
{
    public Optional<string?> OptionalString { get; set; }
}
