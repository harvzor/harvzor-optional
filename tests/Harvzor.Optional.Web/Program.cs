using Harvzor.Optional;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
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
        else
            return "You sent nothing.";
    });

app.Run();

record Foo
{
    public Optional<string?> OptionalString { get; set; }
}
