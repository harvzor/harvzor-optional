using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.TestHost;
using Microsoft.OpenApi.Readers;

namespace Harvzor.Optional.Swashbuckle.Tests;

[Route("/")]
[ApiController]
public class IndexController : ControllerBase
{
    [HttpPost]
    public Optional<string> Post(Optional<string> foo)
    {
        return foo;
    }
}

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();

        services.AddSwaggerGen(options =>
        {
            options.FixOptionalMappings(Assembly.GetExecutingAssembly());
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseRouting();

        app.UseSwagger();
    }
}

public class OptionalSwashbuckleTests
{
    [Fact]
    public async Task SwaggerEndpoint_ShouldOnlyHaveStringTypes_WhenOptionalStringIsInRequestAndResponse()
    {
        // Arrange
        
        var testSite = new TestSite(typeof(Startup));
        var client = testSite.BuildClient();

        // Act
        
        HttpResponseMessage swaggerResponse = await client.GetAsync("/swagger/v1/swagger.json");

        // Assert
        
        swaggerResponse.EnsureSuccessStatusCode();

        // string swaggerContent = await swaggerResponse.Content.ReadAsStringAsync();

        OpenApiDocument? openApiDocument = new OpenApiStreamReader()
            .Read(await swaggerResponse.Content.ReadAsStreamAsync(), out _);

        openApiDocument.Components.Schemas.ShouldNotContainKey("StringOptional");
            
        OpenApiOperation? postOperation = openApiDocument
            .Paths
            .First()
            .Value
            .Operations
            .First(x => x.Key == OperationType.Post)
            .Value;
        
        postOperation.RequestBody
            .Content
            .First(x => x.Key == "application/json")
            .Value
            .Schema
            .Type
            .ShouldBe("string");
        
        postOperation
            .Responses
            .First()
            .Value
            .Content
            .First(x => x.Key == "application/json")
            .Value
            .Schema
            .Type
            .ShouldBe("string");
    }
}

// https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/v6.5.0/test/Swashbuckle.AspNetCore.IntegrationTests/TestSite.cs
public class TestSite
{
    private readonly Type _startupType;

    public TestSite(Type startupType)
    {
        _startupType = startupType;
    }

    private TestServer BuildServer()
    {
        var builder = new WebHostBuilder()
            .UseStartup(_startupType);

        return new TestServer(builder);
    }

    public HttpClient BuildClient()
    {
        var server = BuildServer();
        var client = server.CreateClient();
        client.BaseAddress = new Uri("http://localhost");

        return client;
    }
}
