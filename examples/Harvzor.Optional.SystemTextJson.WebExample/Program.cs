using System.Text.Json.Serialization.Metadata;
using Harvzor.Optional;
using Harvzor.Optional.SystemTextJson;
using Microsoft.AspNetCore.Mvc;
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

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new OptionalJsonConverter());
    options.SerializerOptions.TypeInfoResolver = new DefaultJsonTypeInfoResolver
    {
        Modifiers = { OptionalTypeInfoResolverModifiers.IgnoreUndefinedOptionals }
    };
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app
    .MapGet("/", () => "Hello World!");

app
    .MapPost("/", ([FromBody] Foo foo) =>
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
