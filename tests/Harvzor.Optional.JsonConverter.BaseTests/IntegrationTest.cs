using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;

namespace Harvzor.Optional.JsonConverter.BaseTests;

public class IntegrationTest
{
    /// <example>
    /// <code>
    /// <![CDATA[
    /// public bool Post([FromBody] Optional<bool> foo);
    /// ]]>
    /// </code>
    /// </example>
    [Fact]
    public async void Example()
    {
        // Act

        HttpResponseMessage response = await GetResponse(
            CreateController<FromBodyAttribute>(
                typeof(bool),
                typeof(Optional<bool>)
            ),
           """{"foo": true}"""
        );

        // Assert

        response.EnsureSuccessStatusCode();
    }
    
    private async Task<HttpResponseMessage> GetResponse(Type controller, string content)
    {
        TestStartup.ControllerToUse = controller;

        HttpClient client = new TestSite(typeof(TestStartup))
            .BuildClient();

        // HttpResponseMessage response = await client.GetAsync(
        //     "/?content=" + content
        // );
        
        HttpResponseMessage response = await client.PostAsync(
            "/",
            new StringContent(
                content,
                Encoding.UTF8,
                // "application/x-www-form-urlencoded"
                "application/json"
            )
        );

        response.EnsureSuccessStatusCode();

        return response;
    }

    // Create something like:
    /*
     * [Route("/")]
     * [ApiController]
     * public class IndexController<T> : Controller
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
            typeof(Controller)
        );

        Type routeAttribute = typeof(RouteAttribute);
        Type apiControllerAttribute = typeof(ApiControllerAttribute);

        ConstructorInfo routeCtor = routeAttribute.GetConstructor(new[] { typeof(string) })!;
        ConstructorInfo apiControllerCtor = apiControllerAttribute.GetConstructor(Type.EmptyTypes)!;

        typeBuilder.SetCustomAttribute(new CustomAttributeBuilder(routeCtor, new object[] { "/" }));
        typeBuilder.SetCustomAttribute(new CustomAttributeBuilder(apiControllerCtor, Array.Empty<object>()));

        MethodBuilder methodBuilder = typeBuilder.DefineMethod(
            "Post",
            MethodAttributes.Public | MethodAttributes.Virtual,
            returnType,
            new[] { parameterType }
        );

        Type httpVerbAttribute = typeof(HttpPostAttribute);
        // Type httpVerbAttribute = typeof(HttpGetAttribute);

        ConstructorInfo httpPostCtor = httpVerbAttribute.GetConstructor(Type.EmptyTypes)!;

        methodBuilder.SetCustomAttribute(new CustomAttributeBuilder(httpPostCtor, Array.Empty<object>()));

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
        parameterBuilder.SetCustomAttribute(new CustomAttributeBuilder(fromBodyCtor, Array.Empty<object>()));
        
        ILGenerator ilGenerator = methodBuilder.GetILGenerator();
        // ilGenerator.Emit(OpCodes.Ret); // Trying to return something doesn't work...
        ilGenerator.ThrowException(typeof(NotImplementedException));  // Throw NotImplementedException

        return typeBuilder.CreateType();
    }
}

// [Route("/")]
// [ApiController]
// public class IndexController<T> : ControllerBase
// {
//     [HttpPost]
//     public T Post([FromBody] T foo)
//     {
//         return foo;
//     }
// }

public class TestStartup
{
    public static Type? ControllerToUse { get; set; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();

        services
            .AddMvcCore()
            .UseSpecificControllers(ControllerToUse!);
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseRouting();
        app.UseEndpoints(_ => _.MapDefaultControllerRoute());
        
        // UseEndpoints is used instead:
        // https://stackoverflow.com/questions/74984717/how-to-convert-asp-net-webhost-builder-code-to-net-6-avoid-asp0009-warning-er
        // app.MapDefaultControllerRoute();
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