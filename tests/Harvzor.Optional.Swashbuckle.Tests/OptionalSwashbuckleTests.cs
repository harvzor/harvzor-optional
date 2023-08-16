﻿using System.Net.Mime;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

namespace Harvzor.Optional.Swashbuckle.Tests;

/// <summary>
/// Example controller (which doesn't use <see cref="Optional{T}"/> to compare <see cref="Optional{T}"/>s against.
/// </summary>
[Route("/")]
[ApiController]
public class IndexWithNoOptionalRequestBodyController : ControllerBase
{
    [HttpPost]
    public string Post([FromBody] string foo)
    {
        return foo;
    }
}

[Route("/")]
[ApiController]
public class IndexWithOptionalRequestBodyController : ControllerBase
{
    [HttpPost]
    public string Post([FromBody] Optional<string> foo)
    {
        return foo.Value;
    }
}

[Route("/")]
[ApiController]
public class IndexWithOptionalResponseController : ControllerBase
{
    [HttpPost]
    public Optional<string> Post([FromBody] string foo)
    {
        return foo;
    }
}

[Route("/")]
[ApiController]
public class IndexWithOptionalQueryParamController : ControllerBase
{
    [HttpPost]
    public string Post([FromQuery] Optional<string> foo)
    {
        return foo.Value;
    }
}

public class TestStartup
{
    public static Type? ControllersToUse { get; set; }
    
    public void ConfigureServices(IServiceCollection services)
    {
        // services.AddControllers();
        services
            .AddMvcCore()
            .UseSpecificControllers(ControllersToUse!)
            .AddApiExplorer();

        services.AddSwaggerGen(options =>
        {
            // Just to ensure that the name of the controllers are always the same in Swagger:
            options.TagActionsBy(api => api.RelativePath);
            options.FixOptionalMappings(ControllersToUse!.Assembly);
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
    public async void SwaggerEndpoint_ShouldOnlyHaveStringTypes_WhenOptionalStringIsInRequestBody()
    {
        // Act

        HttpResponseMessage optionalSwaggerResponse = await GetSwaggerResponseForController<IndexWithOptionalRequestBodyController>();
        HttpResponseMessage swaggerResponse = await GetSwaggerResponseForController<IndexWithNoOptionalRequestBodyController>();

        // Assert

        swaggerResponse.EnsureSuccessStatusCode();

        EnsureSwaggerResponsesAreIdentical(optionalSwaggerResponse, swaggerResponse);

        OpenApiDocument openApiDocument = await GetOpenApiDocumentFromResponse(swaggerResponse);

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
            .First(x => x.Key == MediaTypeNames.Application.Json)
            .Value
            .Schema
            .Type
            .ShouldBe("string");

        // Response isn't Optional<T>, but still check it's okay.
        postOperation
            .Responses
            .First()
            .Value
            .Content
            .First(x => x.Key == MediaTypeNames.Application.Json)
            .Value
            .Schema
            .Type
            .ShouldBe("string");
    }
    
    [Fact(Skip = "Turns out Swashbuckle doesn't support a complex type being in a query param, even if I try to map it as a simple type. https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/2226")]
    public async void SwaggerEndpoint_ShouldOnlyHaveStringTypes_WhenOptionalStringIsInRequestQueryParam()
    {
        // Act

        HttpResponseMessage optionalSwaggerResponse = await GetSwaggerResponseForController<IndexWithOptionalQueryParamController>();
        HttpResponseMessage swaggerResponse = await GetSwaggerResponseForController<IndexWithNoOptionalRequestBodyController>();

        // Assert

        swaggerResponse.EnsureSuccessStatusCode();

        EnsureSwaggerResponsesAreIdentical(optionalSwaggerResponse, swaggerResponse);

        OpenApiDocument openApiDocument = await GetOpenApiDocumentFromResponse(swaggerResponse);

        openApiDocument.Components.Schemas.ShouldNotContainKey("StringOptional");

        throw new NotImplementedException("If this ever works, actually check that the doc is correct.");
    }

    [Fact]
    public async void SwaggerEndpoint_ShouldOnlyHaveStringTypes_WhenOptionalStringIsInResponse()
    {
        // Act

        HttpResponseMessage optionalSwaggerResponse = await GetSwaggerResponseForController<IndexWithOptionalResponseController>();
        HttpResponseMessage swaggerResponse = await GetSwaggerResponseForController<IndexWithNoOptionalRequestBodyController>();

        // Assert

        EnsureSwaggerResponsesAreIdentical(optionalSwaggerResponse, swaggerResponse);

        OpenApiDocument openApiDocument = await GetOpenApiDocumentFromResponse(optionalSwaggerResponse);

        openApiDocument.Components.Schemas.ShouldNotContainKey("StringOptional");

        OpenApiOperation? postOperation = openApiDocument
            .Paths
            .First()
            .Value
            .Operations
            .First(x => x.Key == OperationType.Post)
            .Value;

        // Request isn't Optional<T>, but still check it's okay.
        postOperation.RequestBody
            .Content
            .First(x => x.Key == MediaTypeNames.Application.Json)
            .Value
            .Schema
            .Type
            .ShouldBe("string");
        
        postOperation
            .Responses
            .First()
            .Value
            .Content
            .First(x => x.Key == MediaTypeNames.Application.Json)
            .Value
            .Schema
            .Type
            .ShouldBe("string");
    }

    private async void EnsureSwaggerResponsesAreIdentical(HttpResponseMessage optionalSwaggerResponse, HttpResponseMessage swaggerResponse)
    {
        string optionalSwaggerResponseString = await optionalSwaggerResponse.Content.ReadAsStringAsync();
        string swaggerResponseString = await swaggerResponse.Content.ReadAsStringAsync();
        
        optionalSwaggerResponseString.ShouldBe(swaggerResponseString);
    }

    private async Task<OpenApiDocument> GetOpenApiDocumentFromResponse(HttpResponseMessage swaggerResponse)
    {
        return new OpenApiStreamReader()
            .Read(await swaggerResponse.Content.ReadAsStreamAsync(), out _)!;
    }

    private async Task<HttpResponseMessage> GetSwaggerResponseForController<T>() where T : ControllerBase
    {
        TestStartup.ControllersToUse = typeof(T);

        HttpClient client = new TestSite(typeof(TestStartup))
            .BuildClient();

        HttpResponseMessage swaggerResponse = await client.GetAsync("/swagger/v1/swagger.json");
        
        swaggerResponse.EnsureSuccessStatusCode();
        
        return swaggerResponse;
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
        IWebHostBuilder builder = new WebHostBuilder()
            .UseStartup(_startupType);

        return new TestServer(builder);
    }

    public HttpClient BuildClient()
    {
        TestServer server = BuildServer();
        HttpClient client = server.CreateClient();
        client.BaseAddress = new Uri("http://localhost");

        return client;
    }
}

// https://stackoverflow.com/a/68551696/2963111
public static class MvcExtensions
{
    /// <summary>
    /// Finds the appropriate controllers
    /// </summary>
    /// <param name="partManager">The manager for the parts</param>
    /// <param name="controllerTypes">The controller types that are allowed. </param>
    public static void UseSpecificControllers(this ApplicationPartManager partManager, params Type[] controllerTypes)
    {
        partManager.FeatureProviders.Add(new InternalControllerFeatureProvider());
        partManager.ApplicationParts.Clear();
        partManager.ApplicationParts.Add(new SelectedControllersApplicationParts(controllerTypes));
    }

    /// <summary>
    /// Only allow selected controllers
    /// </summary>
    /// <param name="mvcCoreBuilder">The builder that configures mvc core</param>
    /// <param name="controllerTypes">The controller types that are allowed. </param>
    public static IMvcCoreBuilder UseSpecificControllers(this IMvcCoreBuilder mvcCoreBuilder,
        params Type[] controllerTypes)
    {
        return mvcCoreBuilder.ConfigureApplicationPartManager(partManager =>
            partManager.UseSpecificControllers(controllerTypes));
    }

    /// <summary>
    /// Only instantiates selected controllers, not all of them. Prevents application scanning for controllers.
    /// </summary>
    private class SelectedControllersApplicationParts : ApplicationPart, IApplicationPartTypeProvider
    {
        public SelectedControllersApplicationParts(IEnumerable<Type> types)
        {
            Types = types.Select(x => x.GetTypeInfo()).ToArray();
        }

        public override string Name => "";

        public IEnumerable<TypeInfo> Types { get; }
    }

    /// <summary>
    /// Ensure that internal controllers are also allowed. The default ControllerFeatureProvider hides internal
    /// controllers, but this one allows it.
    /// </summary>
    private class InternalControllerFeatureProvider : ControllerFeatureProvider
    {
        private const string ControllerTypeNameSuffix = "Controller";

        /// <summary>
        /// Determines if a given <paramref name="typeInfo" /> is a controller. The default ControllerFeatureProvider hides
        /// internal controllers, but this one allows it.
        /// </summary>
        /// <param name="typeInfo">The <see cref="TypeInfo" /> candidate.</param>
        /// <returns><code>true</code> if the type is a controller; otherwise <code>false</code>.</returns>
        protected override bool IsController(TypeInfo typeInfo)
        {
            if (!typeInfo.IsClass) return false;

            if (typeInfo.IsAbstract) return false;

            if (typeInfo.ContainsGenericParameters) return false;

            if (typeInfo.IsDefined(typeof(NonControllerAttribute))) return false;

            if (!typeInfo.Name.EndsWith(ControllerTypeNameSuffix, StringComparison.OrdinalIgnoreCase) &&
                !typeInfo.IsDefined(typeof(ControllerAttribute)))
                return false;

            return true;
        }
    }
}