using System.Net.Mime;
using System.Reflection;
using System.Reflection.Emit;
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

public class TestStartup
{
    public static Type? ControllerToUse { get; set; }

    public void ConfigureServices(IServiceCollection services)
    {
        // services.AddControllers();
        services
            .AddMvcCore()
            .UseSpecificControllers(ControllerToUse!)
            .AddApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.FixOptionalMappings(ControllerToUse!.Assembly);
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
    /// <example>
    /// <code>
    /// <![CDATA[
    /// public int Post([FromBody] Optional<int> foo);
    /// ]]>
    /// </code>
    /// </example>
    [Theory]
    [InlineData(typeof(int), typeof(Optional<int>), "IntOptional", "integer", "int32")]
    [InlineData(typeof(int?), typeof(Optional<int?>), "IntNullableOptional", "integer", "int32")]
    [InlineData(typeof(long), typeof(Optional<long>), "LongOptional", "integer", "int64")]
    [InlineData(typeof(long?), typeof(Optional<long?>), "LongNullableOptional", "integer", "int64")]
    [InlineData(typeof(float), typeof(Optional<float>), "FloatOptional", "number", "float")]
    [InlineData(typeof(float?), typeof(Optional<float?>), "FloatNullableOptional", "number", "float")]
    [InlineData(typeof(double), typeof(Optional<double>), "DoubleOptional", "number", "double")]
    [InlineData(typeof(double?), typeof(Optional<double?>), "DoubleNullableOptional", "number", "double")]
    [InlineData(typeof(string), typeof(Optional<string>), "StringOptional", "string", null)]
    [InlineData(typeof(bool), typeof(Optional<bool>), "BoolOptional", "boolean", null)]
    [InlineData(typeof(bool?), typeof(Optional<bool?>), "BoolNullableOptional", "boolean", null)]
    [InlineData(typeof(DateTime), typeof(Optional<DateTime>), "DateTimeOptional", "string", "date-time")]
    [InlineData(typeof(DateTime?), typeof(Optional<DateTime?>), "DateTimeNullableOptional", "string", "date-time")]
    // [InlineData(typeof(int[]), typeof(Optional<int[]>), "Int32ArrayOptional", "array", null)]
    public async void SwaggerEndpoint_ShouldOnlyHaveGenericTypes_WhenOptionalTypeIsInRequestBody(Type type, Type optionalType, string shouldNotContainKey, string typeShouldBe, string formatShouldBe)
    {
        // Act

        HttpResponseMessage optionalSwaggerResponse = await GetSwaggerResponseForController(
            CreateController<FromBodyAttribute>(
                type,
                optionalType
            )
        );
        HttpResponseMessage swaggerResponse = await GetSwaggerResponseForController(
            CreateController<FromBodyAttribute>(
                type,
                type
            )
        );

        // Assert

        swaggerResponse.EnsureSuccessStatusCode();

        await EnsureSwaggerResponsesAreIdentical(optionalSwaggerResponse, swaggerResponse);

        OpenApiDocument openApiDocument = await GetOpenApiDocumentFromResponse(swaggerResponse);

        openApiDocument.Components.Schemas.ShouldNotContainKey(shouldNotContainKey);

        OpenApiOperation? postOperation = openApiDocument
            .Paths
            .First()
            .Value
            .Operations
            .First(x => x.Key == OperationType.Post)
            .Value;

        OpenApiSchema? requestBodySchema = postOperation.RequestBody
            .Content
            .First(x => x.Key == MediaTypeNames.Application.Json)
            .Value
            .Schema;
            
        requestBodySchema.Type.ShouldBe(typeShouldBe);
        requestBodySchema.Format.ShouldBe(formatShouldBe);

        // Response isn't Optional<T>, but still check it's okay.
        OpenApiSchema? responseBodySchema = postOperation
            .Responses
            .First()
            .Value
            .Content
            .First(x => x.Key == MediaTypeNames.Application.Json)
            .Value
            .Schema;
        
        responseBodySchema.Type.ShouldBe(typeShouldBe);
        responseBodySchema.Format.ShouldBe(formatShouldBe);
    }
    
    /// <example>
    /// <code>
    /// <![CDATA[
    /// public int[] Post([FromBody] Optional<int[]> foo);
    /// ]]>
    /// </code>
    /// </example>
    [Theory]
    [InlineData(typeof(int[]), typeof(Optional<int[]>))]
    [InlineData(typeof(IEnumerable<int>), typeof(Optional<IEnumerable<int>>))]
    [InlineData(typeof(List<int>), typeof(Optional<List<int>>))]
    public async void SwaggerEndpoint_ShouldOnlyHaveGenericTypes_WhenOptionalArrayTypeIsInRequestBody(Type type, Type optionalType)
    {
        // Act

        HttpResponseMessage optionalSwaggerResponse = await GetSwaggerResponseForController(
            CreateController<FromBodyAttribute>(
                type,
                optionalType
            )
        );
        HttpResponseMessage swaggerResponse = await GetSwaggerResponseForController(
            CreateController<FromBodyAttribute>(
                type,
                type
            )
        );

        // Assert

        swaggerResponse.EnsureSuccessStatusCode();

        await EnsureSwaggerResponsesAreIdentical(optionalSwaggerResponse, swaggerResponse);

        OpenApiDocument openApiDocument = await GetOpenApiDocumentFromResponse(swaggerResponse);

        openApiDocument.Components.Schemas.ShouldNotContainKey("StringNullableOptional");

        OpenApiOperation? postOperation = openApiDocument
            .Paths
            .First()
            .Value
            .Operations
            .First(x => x.Key == OperationType.Post)
            .Value;

        OpenApiSchema? requestBodySchema = postOperation.RequestBody
            .Content
            .First(x => x.Key == MediaTypeNames.Application.Json)
            .Value
            .Schema;
            
        requestBodySchema.Type.ShouldBe("array");
        requestBodySchema.Format.ShouldBe(null);
        
        requestBodySchema.Items.ShouldNotBe(null);
        requestBodySchema.Items.Type.ShouldBe("integer");
        requestBodySchema.Items.Format.ShouldBe("int32");

        // Response isn't Optional<T>, but still check it's okay.
        OpenApiSchema? responseBodySchema = postOperation
            .Responses
            .First()
            .Value
            .Content
            .First(x => x.Key == MediaTypeNames.Application.Json)
            .Value
            .Schema;
        
        responseBodySchema.Type.ShouldBe("array");
        responseBodySchema.Format.ShouldBe(null);
        
        requestBodySchema.Items.ShouldNotBe(null);
        requestBodySchema.Items.Type.ShouldBe("integer");
        requestBodySchema.Items.Format.ShouldBe("int32");
    }
    
    /// <example>
    /// <code>
    /// <![CDATA[
    /// public int[][] Post([FromBody] Optional<int[][]> foo);
    /// ]]>
    /// </code>
    /// </example>
    [Fact]
    public async void SwaggerEndpoint_ShouldOnlyHaveGenericTypes_WhenOptionalArrayOfArrayTypeIsInRequestBody()
    {
        // Act

        HttpResponseMessage optionalSwaggerResponse = await GetSwaggerResponseForController(
            CreateController<FromBodyAttribute>(
                typeof(int[][]),
                typeof(Optional<int[][]>)
            )
        );
        HttpResponseMessage swaggerResponse = await GetSwaggerResponseForController(
            CreateController<FromBodyAttribute>(
                typeof(int[][]),
                typeof(int[][])
            )
        );

        // Assert

        swaggerResponse.EnsureSuccessStatusCode();

        await EnsureSwaggerResponsesAreIdentical(optionalSwaggerResponse, swaggerResponse);

        OpenApiDocument openApiDocument = await GetOpenApiDocumentFromResponse(swaggerResponse);

        openApiDocument.Components.Schemas.ShouldNotContainKey("StringNullableOptional");

        OpenApiOperation? postOperation = openApiDocument
            .Paths
            .First()
            .Value
            .Operations
            .First(x => x.Key == OperationType.Post)
            .Value;

        OpenApiSchema? requestBodySchema = postOperation.RequestBody
            .Content
            .First(x => x.Key == MediaTypeNames.Application.Json)
            .Value
            .Schema;
            
        requestBodySchema.Type.ShouldBe("array");
        requestBodySchema.Format.ShouldBe(null);
        
        requestBodySchema.Items.ShouldNotBe(null);
        requestBodySchema.Items.Type.ShouldBe("array");
        requestBodySchema.Items.Format.ShouldBe(null);
        
        requestBodySchema.Items.Items.ShouldNotBe(null);
        requestBodySchema.Items.Items.Type.ShouldBe("integer");
        requestBodySchema.Items.Items.Format.ShouldBe("int32");

        // Response isn't Optional<T>, but still check it's okay.
        OpenApiSchema? responseBodySchema = postOperation
            .Responses
            .First()
            .Value
            .Content
            .First(x => x.Key == MediaTypeNames.Application.Json)
            .Value
            .Schema;
        
        responseBodySchema.Type.ShouldBe("array");
        responseBodySchema.Format.ShouldBe(null);
        
        responseBodySchema.Items.ShouldNotBe(null);
        responseBodySchema.Items.Type.ShouldBe("array");
        responseBodySchema.Items.Format.ShouldBe(null);
        
        responseBodySchema.Items.Items.ShouldNotBe(null);
        responseBodySchema.Items.Items.Type.ShouldBe("integer");
        responseBodySchema.Items.Items.Format.ShouldBe("int32");
    }

    /// <example>
    /// <code>
    /// <![CDATA[
    /// public string Post([FromQuery] Optional<string> foo);
    /// ]]>
    /// </code>
    /// </example>
    [Fact(Skip =
        "Turns out Swashbuckle doesn't support a complex type being in a query param, even if I try to map it as a simple type. https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/2226")]
    public async void SwaggerEndpoint_ShouldOnlyHaveStringTypes_WhenOptionalStringIsInRequestQueryParam()
    {
        // Act

        HttpResponseMessage optionalSwaggerResponse = await GetSwaggerResponseForController(
            CreateController<FromQueryAttribute>(
                typeof(string),
                typeof(Optional<string>)
            )
        );
        HttpResponseMessage swaggerResponse = await GetSwaggerResponseForController(
            CreateController<FromQueryAttribute>(
                typeof(string),
                typeof(string)
            )
        );

        // Assert

        swaggerResponse.EnsureSuccessStatusCode();

        await EnsureSwaggerResponsesAreIdentical(optionalSwaggerResponse, swaggerResponse);

        OpenApiDocument openApiDocument = await GetOpenApiDocumentFromResponse(swaggerResponse);

        openApiDocument.Components.Schemas.ShouldNotContainKey("StringOptional");

        throw new NotImplementedException("If this ever works, actually check that the doc is correct.");
    }

    /// <example>
    /// <code>
    /// <![CDATA[
    /// public Optional<string> Post([FromBody] string foo);
    /// ]]>
    /// </code>
    /// </example>
    [Fact]
    public async void SwaggerEndpoint_ShouldOnlyHaveStringTypes_WhenOptionalStringIsInResponse()
    {
        // Act

        HttpResponseMessage optionalSwaggerResponse = await GetSwaggerResponseForController(
            CreateController<FromBodyAttribute>(
                typeof(Optional<string>), 
                typeof(string)
            )
        );
        HttpResponseMessage swaggerResponse = await GetSwaggerResponseForController(
            CreateController<FromBodyAttribute>(
                typeof(string),
                typeof(string)
            )
        );

        // Assert

        await EnsureSwaggerResponsesAreIdentical(optionalSwaggerResponse, swaggerResponse);

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
    
    /// <summary>
    /// Often <see cref="IActionResult"/> is the return type for a controller method, but the actual response type is
    /// defined in the <see cref="ProducesResponseTypeAttribute"/>.
    /// </summary>
    /// <example>
    /// <code>
    /// <![CDATA[
    /// [ProducesResponseType(typeof(Optional<string>), 200)]
    /// [ProducesResponseType(typeof(Optional<int>), 201)]
    /// public IActionResult Post([FromBody] string foo);
    /// ]]>
    /// </code>
    /// </example>
    [Fact]
    public async void SwaggerEndpoint_ShouldOnlyHaveStringAndIntTypes_WhenOptionalStringAndIntIsDefinedInProducesResponseTypeAttributes()
    {
        // Act

        HttpResponseMessage optionalSwaggerResponse = await GetSwaggerResponseForController(
            CreateController<FromBodyAttribute>(
                typeof(IActionResult), 
                typeof(string),
                new []{ (typeof(Optional<string>), 200), (typeof(Optional<int>), 201) }
            )
        );
        HttpResponseMessage swaggerResponse = await GetSwaggerResponseForController(
            CreateController<FromBodyAttribute>(
                typeof(IActionResult),
                typeof(string),
                new []{ (typeof(string), 200), (typeof(int), 201) }
            )
        );

        // Assert

        await EnsureSwaggerResponsesAreIdentical(optionalSwaggerResponse, swaggerResponse);

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
            .First(x => x.Key == "200")
            .Value
            .Content
            .First(x => x.Key == MediaTypeNames.Application.Json)
            .Value
            .Schema
            .Type
            .ShouldBe("string");
        
        postOperation
            .Responses
            .First(x => x.Key == "201")
            .Value
            .Content
            .First(x => x.Key == MediaTypeNames.Application.Json)
            .Value
            .Schema
            .Type
            .ShouldBe("integer");
    }

    private class Foo
    {
        public string Bar { get; set; }
    }
    
    /// <example>
    /// <code>
    /// <![CDATA[
    /// public Foo Post([FromBody] Optional<Foo> foo);
    /// ]]>
    /// </code>
    /// </example>
    [Fact]
    public async void SwaggerEndpoint_ShouldOnlyHaveObjects_WhenOptionalObjectIsInRequest()
    {
        // Act

        HttpResponseMessage optionalSwaggerResponse = await GetSwaggerResponseForController(
            CreateController<FromBodyAttribute>(
                typeof(Foo), 
                typeof(Optional<Foo>)
            )
        );
        HttpResponseMessage swaggerResponse = await GetSwaggerResponseForController(
            CreateController<FromBodyAttribute>(
                typeof(Foo), 
                typeof(Foo)
            )
        );

        // Assert

        await EnsureSwaggerResponsesAreIdentical(optionalSwaggerResponse, swaggerResponse);

        OpenApiDocument openApiDocument = await GetOpenApiDocumentFromResponse(optionalSwaggerResponse);

        OpenApiOperation? postOperation = openApiDocument
            .Paths
            .First()
            .Value
            .Operations
            .First(x => x.Key == OperationType.Post)
            .Value;

        OpenApiSchema requestSchema = postOperation.RequestBody
            .Content
            .First(x => x.Key == MediaTypeNames.Application.Json)
            .Value
            .Schema;
            
        requestSchema.Type.ShouldBe("object");
        requestSchema.Reference.ReferenceV3.ShouldBe($"#/components/schemas/{nameof(Foo)}");

        OpenApiSchema responseSchema = postOperation
            .Responses
            .First()
            .Value
            .Content
            .First(x => x.Key == MediaTypeNames.Application.Json)
            .Value
            .Schema;
        
        responseSchema.Type.ShouldBe("object");
        responseSchema.Reference.ReferenceV3.ShouldBe($"#/components/schemas/{nameof(Foo)}");

        OpenApiSchema fooSchema = openApiDocument
            .Components
            .Schemas
            .First(x => x.Key == $"{nameof(Foo)}")
            .Value;

        fooSchema
            .Properties
            .First(x => x.Key == nameof(Foo.Bar).ToLower())
            .Value
            .Type
            .ShouldBe("string");
    }

    // Wrap so the version which uses Optional<T> can have the same name as the one that doesn't.
    private class WrapperOptional
    {
        public class Baz
        {
            public Optional<Foo> Foo { get; set; }
        }
    }
    
    private class Baz
    {
        public Foo Foo { get; set; }
    }

    /// <example>
    /// <code>
    /// <![CDATA[
    /// public Foo Post([FromBody] Bar foo);
    /// ]]>
    /// </code>
    /// </example>
    [Fact]
    public async void SwaggerEndpoint_ShouldCorrectlyMapNestedObjects_WhenOptionalObjectIsinObjectInRequest()
    {
        // Act

        HttpResponseMessage optionalSwaggerResponse = await GetSwaggerResponseForController(
            CreateController<FromBodyAttribute>(
                typeof(WrapperOptional.Baz), 
                typeof(WrapperOptional.Baz)
            )
        );
        HttpResponseMessage swaggerResponse = await GetSwaggerResponseForController(
            CreateController<FromBodyAttribute>(
                typeof(Baz), 
                typeof(Baz)
            )
        );

        // Assert

        await EnsureSwaggerResponsesAreIdentical(optionalSwaggerResponse, swaggerResponse);

        OpenApiDocument openApiDocument = await GetOpenApiDocumentFromResponse(optionalSwaggerResponse);
        
        OpenApiOperation? postOperation = openApiDocument
            .Paths
            .First()
            .Value
            .Operations
            .First(x => x.Key == OperationType.Post)
            .Value;
        
        OpenApiSchema requestSchema = postOperation.RequestBody
            .Content
            .First(x => x.Key == MediaTypeNames.Application.Json)
            .Value
            .Schema;
            
        requestSchema.Type.ShouldBe("object");
        requestSchema.Reference.ReferenceV3.ShouldBe($"#/components/schemas/{nameof(WrapperOptional.Baz)}");
        
        OpenApiSchema responseSchema = postOperation
            .Responses
            .First()
            .Value
            .Content
            .First(x => x.Key == MediaTypeNames.Application.Json)
            .Value
            .Schema;
        
        responseSchema.Type.ShouldBe("object");
        responseSchema.Reference.ReferenceV3.ShouldBe($"#/components/schemas/{nameof(WrapperOptional.Baz)}");
        
        OpenApiSchema bazSchema = openApiDocument
            .Components
            .Schemas
            .First(x => x.Key == $"{nameof(WrapperOptional.Baz)}")
            .Value;

        bazSchema
            .Properties
            .First(x => x.Key == nameof(WrapperOptional.Baz.Foo).ToLower())
            .Value
            .Reference
            .ReferenceV3
            .ShouldBe($"#/components/schemas/{nameof(Foo)}");

        OpenApiSchema fooSchema = openApiDocument
            .Components
            .Schemas
            .First(x => x.Key == $"{nameof(Baz.Foo)}")
            .Value;
        
        fooSchema
            .Properties
            .First(x => x.Key == nameof(Foo.Bar).ToLower())
            .Value
            .Type
            .ShouldBe("string");
    }
    
    // todo: check FixOptionalMappingForType works

    [Fact]
    public void FixOptionalMappings_ShouldThrowException_WhenNoAssemblyProvided()
    {
        // Arrange
        
        IServiceCollection? services = null;
        
        // Act

        Func<IServiceCollection> action = () => services.AddSwaggerGen();
        
        // Assert

        action.ShouldThrow<ArgumentException>();
    }

    private async Task EnsureSwaggerResponsesAreIdentical(HttpResponseMessage optionalSwaggerResponse,
        HttpResponseMessage swaggerResponse)
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

    private async Task<HttpResponseMessage> GetSwaggerResponseForController(Type controller)
    {
        TestStartup.ControllerToUse = controller;

        HttpClient client = new TestSite(typeof(TestStartup))
            .BuildClient();

        HttpResponseMessage swaggerResponse = await client.GetAsync("/swagger/v1/swagger.json");

        swaggerResponse.EnsureSuccessStatusCode();

        return swaggerResponse;
    }

    // Create something like:
    /*
     * [Route("/")]
     * [ApiController]
     * public class IndexWithNoOptionalRequestBodyController<T> : ControllerBase
     * {
     *     [HttpPost]
     *     public T Post([FromBody] T foo)
     *     {
     *         return foo;
     *     }
     * }
     */
    private Type CreateController<TParameterAttribute>(Type returnType, Type parameterType, (Type, int)[]? producesResponseTypes = null)
        where TParameterAttribute : Attribute
    {
        AssemblyName assemblyName = new AssemblyName("DynamicAssembly");
        AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("DynamicModule");

        TypeBuilder typeBuilder = moduleBuilder.DefineType(
            "IndexController",
            TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoClass | TypeAttributes.AnsiClass |
            TypeAttributes.BeforeFieldInit | TypeAttributes.AutoLayout,
            typeof(ControllerBase)
        );

        Type routeAttribute = typeof(RouteAttribute);
        Type apiControllerAttribute = typeof(ApiControllerAttribute);

        ConstructorInfo routeCtor = routeAttribute.GetConstructor(new[] { typeof(string) })!;
        ConstructorInfo apiControllerCtor = apiControllerAttribute.GetConstructor(Type.EmptyTypes)!;

        typeBuilder.SetCustomAttribute(new CustomAttributeBuilder(routeCtor, new object[] { "/" }));
        typeBuilder.SetCustomAttribute(new CustomAttributeBuilder(apiControllerCtor, new object[0]));

        MethodBuilder methodBuilder = typeBuilder.DefineMethod(
            "Post",
            MethodAttributes.Public | MethodAttributes.Virtual,
            returnType,
            new[] { parameterType }
        );

        Type httpPostAttribute = typeof(HttpPostAttribute);

        ConstructorInfo httpPostCtor = httpPostAttribute.GetConstructor(Type.EmptyTypes)!;

        methodBuilder.SetCustomAttribute(new CustomAttributeBuilder(httpPostCtor, new object[0]));

        if (producesResponseTypes != null)
        {
            foreach ((Type type, int statusCode) in producesResponseTypes)
            {
                Type producesResponseTypeAttribute = typeof(ProducesResponseTypeAttribute);

                ConstructorInfo producesResponseTypeCtor = producesResponseTypeAttribute.GetConstructor(new[] { typeof(Type), typeof(int) })!;
            
                methodBuilder.SetCustomAttribute(new CustomAttributeBuilder(producesResponseTypeCtor, new object[] { type, statusCode }));
            }
        }

        Type fromBodyAttribute = typeof(TParameterAttribute);
        ConstructorInfo? fromBodyCtor = fromBodyAttribute.GetConstructor(Type.EmptyTypes);

        ParameterBuilder parameterBuilder = methodBuilder.DefineParameter(1, ParameterAttributes.None, "foo");
        parameterBuilder.SetCustomAttribute(new CustomAttributeBuilder(fromBodyCtor, new object[0]));

        ILGenerator ilGenerator = methodBuilder.GetILGenerator();
        ilGenerator.ThrowException(typeof(NotImplementedException));  // Throw NotImplementedException
        
        return typeBuilder.CreateType();
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