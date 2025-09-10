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
    /// <remarks>
    /// Should equal list from https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/8f363f7359cb1cb8fa5de5195ec6d97aefaa16b3/test/Swashbuckle.AspNetCore.SwaggerGen.Test/SchemaGenerator/JsonSerializerSchemaGeneratorTests.cs#L35.
    /// </remarks>
    [Theory]
    // [InlineData(typeof(bool), "boolean", null)]
    [InlineData(typeof(bool), typeof(Optional<bool>), "BoolOptional", "boolean", null)]
    [InlineData(typeof(bool?), typeof(Optional<bool?>), "BoolNullableOptional", "boolean", null)]
    
    // [InlineData(typeof(byte), "integer", "int32")]
    [InlineData(typeof(byte), typeof(Optional<byte>), "IntOptional", "integer", "int32")]
    [InlineData(typeof(byte?), typeof(Optional<byte?>), "IntNullableOptional", "integer", "int32")]

    // [InlineData(typeof(sbyte), "integer", "int32")]
    [InlineData(typeof(sbyte), typeof(Optional<sbyte>), "IntOptional", "integer", "int32")]
    [InlineData(typeof(sbyte?), typeof(Optional<sbyte?>), "IntNullableOptional", "integer", "int32")]
    
    // [InlineData(typeof(short), "integer", "int32")]
    [InlineData(typeof(short), typeof(Optional<short>), "IntOptional", "integer", "int32")]
    [InlineData(typeof(short?), typeof(Optional<short?>), "IntNullableOptional", "integer", "int32")]
    
    // [InlineData(typeof(ushort), "integer", "int32")]
    [InlineData(typeof(ushort), typeof(Optional<ushort>), "IntOptional", "integer", "int32")]
    [InlineData(typeof(ushort?), typeof(Optional<ushort?>), "IntNullableOptional", "integer", "int32")]
    
    // [InlineData(typeof(int), "integer", "int32")]
    [InlineData(typeof(int), typeof(Optional<int>), "IntOptional", "integer", "int32")]
    [InlineData(typeof(int?), typeof(Optional<int?>), "IntNullableOptional", "integer", "int32")]
    
    // [InlineData(typeof(uint), "integer", "int32")]
    [InlineData(typeof(uint), typeof(Optional<uint>), "IntOptional", "integer", "int32")]
    [InlineData(typeof(uint?), typeof(Optional<uint?>), "IntNullableOptional", "integer", "int32")]
    
    // [InlineData(typeof(long), "integer", "int64")]
    [InlineData(typeof(long), typeof(Optional<long>), "LongOptional", "integer", "int64")]
    [InlineData(typeof(long?), typeof(Optional<long?>), "LongNullableOptional", "integer", "int64")]
    
    // [InlineData(typeof(ulong), "integer", "int64")]
    [InlineData(typeof(ulong), typeof(Optional<ulong>), "IntOptional", "integer", "int64")]
    [InlineData(typeof(ulong?), typeof(Optional<ulong?>), "IntNullableOptional", "integer", "int64")]
    
    // [InlineData(typeof(float), "number", "float")]
    [InlineData(typeof(float), typeof(Optional<float>), "FloatOptional", "number", "float")]
    [InlineData(typeof(float?), typeof(Optional<float?>), "FloatNullableOptional", "number", "float")]
    
    // [InlineData(typeof(double), "number", "double")]
    [InlineData(typeof(double), typeof(Optional<double>), "DoubleOptional", "number", "double")]
    [InlineData(typeof(double?), typeof(Optional<double?>), "DoubleNullableOptional", "number", "double")]
    
    // [InlineData(typeof(decimal), "number", "double")]
    [InlineData(typeof(decimal), typeof(Optional<decimal>), "DecimalOptional", "number", "double")]
    [InlineData(typeof(decimal?), typeof(Optional<decimal?>), "DecimalNullableOptional", "number", "double")]
    
    // [InlineData(typeof(string), "string", null)]
    [InlineData(typeof(string), typeof(Optional<string>), "StringOptional", "string", null)]
    // [InlineData(typeof(string?), typeof(Optional<string?>), "StringOptional", "string", null)]
    
    // [InlineData(typeof(char), "string", null)]
    [InlineData(typeof(char), typeof(Optional<char>), "StringOptional", "string", null)]
    [InlineData(typeof(char?), typeof(Optional<char?>), "StringOptional", "string", null)]
    
    // [InlineData(typeof(byte[]), "string", "byte")]
    [InlineData(typeof(byte[]), typeof(Optional<byte[]>), "StringOptional", "string", "byte")]
    // [InlineData(typeof(byte[]?), typeof(Optional<byte[]?>), "StringOptional", "string", "byte")]
    
    // [InlineData(typeof(DateTime), "string", "date-time")]
    [InlineData(typeof(DateTime), typeof(Optional<DateTime>), "DateTimeOptional", "string", "date-time")]
    [InlineData(typeof(DateTime?), typeof(Optional<DateTime?>), "DateTimeNullableOptional", "string", "date-time")]
    
    // [InlineData(typeof(DateTimeOffset), "string", "date-time")]
    [InlineData(typeof(DateTimeOffset), typeof(Optional<DateTimeOffset>), "DateTimeOffsetOptional", "string", "date-time")]
    [InlineData(typeof(DateTimeOffset?), typeof(Optional<DateTimeOffset?>), "DateTimeOffsetNullableOptional", "string", "date-time")]
    
    // [InlineData(typeof(Guid), "string", "uuid")]
    [InlineData(typeof(Guid), typeof(Optional<Guid>), "GuidOptional", "string", "uuid")]
    [InlineData(typeof(Guid?), typeof(Optional<Guid?>), "GuidNullableOptional", "string", "uuid")]
    
    // [InlineData(typeof(Uri), "string", "uri")]
    [InlineData(typeof(Uri), typeof(Optional<Uri>), "UriOptional", "string", "uri")]
    // [InlineData(typeof(Uri?), typeof(Optional<Uri?>), "UriNullableOptional", "string", "uri")]
    
    // [InlineData(typeof(DateOnly), "string", "date")]
    [InlineData(typeof(DateOnly), typeof(Optional<DateOnly>), "DateOnlyOptional", "string", "date")]
    [InlineData(typeof(DateOnly?), typeof(Optional<DateOnly?>), "DateOnlyNullableOptional", "string", "date")]
    
    // [InlineData(typeof(TimeOnly), "string", "time")]
    [InlineData(typeof(TimeOnly), typeof(Optional<TimeOnly>), "TimeOnlyOptional", "string", "time")]
    [InlineData(typeof(TimeOnly?), typeof(Optional<TimeOnly?>), "TimeOnlyNullableOptional", "string", "time")]

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
        HttpResponseMessage expectedSwaggerResponse = await GetSwaggerResponseForController(
            CreateController<FromBodyAttribute>(
                type,
                type
            )
        );

        // Assert

        expectedSwaggerResponse.EnsureSuccessStatusCode();

        await EnsureSwaggerResponsesAreIdentical(optionalSwaggerResponse, expectedSwaggerResponse);

        OpenApiDocument openApiDocument = await GetOpenApiDocumentFromResponse(expectedSwaggerResponse);

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
        HttpResponseMessage expectedSwaggerResponse = await GetSwaggerResponseForController(
            CreateController<FromBodyAttribute>(
                type,
                type
            )
        );

        // Assert

        expectedSwaggerResponse.EnsureSuccessStatusCode();

        await EnsureSwaggerResponsesAreIdentical(optionalSwaggerResponse, expectedSwaggerResponse);

        OpenApiDocument openApiDocument = await GetOpenApiDocumentFromResponse(expectedSwaggerResponse);

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
        HttpResponseMessage expectedSwaggerResponse = await GetSwaggerResponseForController(
            CreateController<FromBodyAttribute>(
                typeof(int[][]),
                typeof(int[][])
            )
        );

        // Assert

        expectedSwaggerResponse.EnsureSuccessStatusCode();

        await EnsureSwaggerResponsesAreIdentical(optionalSwaggerResponse, expectedSwaggerResponse);

        OpenApiDocument openApiDocument = await GetOpenApiDocumentFromResponse(expectedSwaggerResponse);

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
        HttpResponseMessage expectedSwaggerResponse = await GetSwaggerResponseForController(
            CreateController<FromQueryAttribute>(
                typeof(string),
                typeof(string)
            )
        );

        // Assert

        expectedSwaggerResponse.EnsureSuccessStatusCode();

        await EnsureSwaggerResponsesAreIdentical(optionalSwaggerResponse, expectedSwaggerResponse);

        OpenApiDocument openApiDocument = await GetOpenApiDocumentFromResponse(expectedSwaggerResponse);

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
        HttpResponseMessage expectedSwaggerResponse = await GetSwaggerResponseForController(
            CreateController<FromBodyAttribute>(
                typeof(string),
                typeof(string)
            )
        );

        // Assert

        await EnsureSwaggerResponsesAreIdentical(optionalSwaggerResponse, expectedSwaggerResponse);

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
        HttpResponseMessage expectedSwaggerResponse = await GetSwaggerResponseForController(
            CreateController<FromBodyAttribute>(
                typeof(IActionResult),
                typeof(string),
                new []{ (typeof(string), 200), (typeof(int), 201) }
            )
        );

        // Assert

        await EnsureSwaggerResponsesAreIdentical(optionalSwaggerResponse, expectedSwaggerResponse);

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
        HttpResponseMessage expectedSwaggerResponse = await GetSwaggerResponseForController(
            CreateController<FromBodyAttribute>(
                typeof(Foo), 
                typeof(Foo)
            )
        );

        // Assert

        await EnsureSwaggerResponsesAreIdentical(optionalSwaggerResponse, expectedSwaggerResponse);

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

    // WrapperOptional wraps the class inside so there can be two classes called the same, so an expected swagger response
    // can be generated with the same class names.
    private partial class WrapperOptional
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
    /// public Baz Post([FromBody] Baz baz);
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
        HttpResponseMessage expectedSwaggerResponse = await GetSwaggerResponseForController(
            CreateController<FromBodyAttribute>(
                typeof(Baz), 
                typeof(Baz)
            )
        );

        // Assert

        await EnsureSwaggerResponsesAreIdentical(optionalSwaggerResponse, expectedSwaggerResponse);

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


    // WrapperOptional wraps the class inside so there can be two classes called the same, so an expected swagger response
    // can be generated with the same class names.
    private partial class WrapperOptional
    {
        public class Qux
        {
            public Optional<Quux> Quux { get; set; }
        }

        public class Quux
        {
            public Optional<Qux> Qux { get; set; }
        }
    }
    
    public class Qux
    {
        public Optional<Quux> Quux { get; set; }
    }

    public class Quux
    {
        public Optional<Qux> Qux { get; set; }
    }

    /// <summary>https://github.com/harvzor/harvzor-optional/issues/11#issuecomment-3270140217</summary>
    /// <example>
    /// <code>
    /// <![CDATA[
    /// public Qux Post([FromBody] Qux qux);
    /// ]]>
    /// </code>
    /// </example>
    [Fact]
    public async void SwaggerEndpoint_ShouldCorrectlyMapObjectsThatContainCyclicalReferencesToOptionalObjects_WhenObjectInRequest()
    {
        // Act

        HttpResponseMessage optionalSwaggerResponse = await GetSwaggerResponseForController(
            CreateController<FromBodyAttribute>(
                typeof(WrapperOptional.Qux),
                typeof(WrapperOptional.Qux)
            )
        );
        HttpResponseMessage expectedSwaggerResponse = await GetSwaggerResponseForController(
            CreateController<FromBodyAttribute>(
                typeof(Qux), 
                typeof(Qux)
            )
        );

        // Assert

        await EnsureSwaggerResponsesAreIdentical(optionalSwaggerResponse, expectedSwaggerResponse);

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
        requestSchema.Reference.ReferenceV3.ShouldBe($"#/components/schemas/{nameof(WrapperOptional.Qux)}");
        
        OpenApiSchema responseSchema = postOperation
            .Responses
            .First()
            .Value
            .Content
            .First(x => x.Key == MediaTypeNames.Application.Json)
            .Value
            .Schema;
        
        responseSchema.Type.ShouldBe("object");
        responseSchema.Reference.ReferenceV3.ShouldBe($"#/components/schemas/{nameof(WrapperOptional.Qux)}");
        
        OpenApiSchema quxSchema = openApiDocument
            .Components
            .Schemas
            .First(x => x.Key == $"{nameof(WrapperOptional.Qux)}")
            .Value;

        quxSchema
            .Properties
            .First(x => x.Key == nameof(WrapperOptional.Qux.Quux).ToLower())
            .Value
            .Reference
            .ReferenceV3
            .ShouldBe($"#/components/schemas/{nameof(Quux)}");

        OpenApiSchema quuxSchema = openApiDocument
            .Components
            .Schemas
            .First(x => x.Key == $"{nameof(Qux.Quux)}")
            .Value;
        
        quuxSchema
            .Properties
            .First(x => x.Key == nameof(Qux).ToLower())
            .Value
            .Reference
            .ReferenceV3
            .ShouldBe($"#/components/schemas/{nameof(Qux)}");
    }

    // Only a struct can really be Nullable.
    private struct FooStruct
    {
    }

    /// <example>
    /// <code>
    /// <![CDATA[
    /// public FooStruct Post([FromBody] Optional<FooStruct?> fooStruct);
    /// ]]>
    /// </code>
    /// </example>
    [Fact]
    public async void SwaggerEndpoint_ShouldCorrectlyMapOptionalNullableStructs_WhenOptionalNullableStructIsInRequest()
    {
        // Act

        HttpResponseMessage optionalSwaggerResponse = await GetSwaggerResponseForController(
            CreateController<FromBodyAttribute>(
                typeof(FooStruct),
                typeof(Optional<FooStruct?>)
            )
        );
        HttpResponseMessage swaggerResponse = await GetSwaggerResponseForController(
            CreateController<FromBodyAttribute>(
                typeof(FooStruct), 
                typeof(FooStruct)
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
        requestSchema.Reference.ReferenceV3.ShouldBe($"#/components/schemas/{nameof(FooStruct)}");

        openApiDocument
            .Components
            .Schemas
            .ShouldContain(x => x.Key == $"{nameof(FooStruct)}");
    }
    
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
        HttpResponseMessage expectedSwaggerResponse)
    {
        string optionalSwaggerResponseString = await optionalSwaggerResponse.Content.ReadAsStringAsync();
        string swaggerResponseString = await expectedSwaggerResponse.Content.ReadAsStringAsync();

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