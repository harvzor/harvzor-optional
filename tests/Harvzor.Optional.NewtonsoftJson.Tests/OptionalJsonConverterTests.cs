using Harvzor.Optional.JsonConverter.BaseTests;
using Newtonsoft.Json;

namespace Harvzor.Optional.NewtonsoftJson.Tests;

public class OptionalJsonConverterTests : OptionalJsonConverterBaseTests
{
    protected override T Deserialize<T>(string str)
    {
        return JsonConvert.DeserializeObject<T>(str, new OptionalJsonConverter())!;
    }

    protected override string Serialize(object obj)
    {
        return JsonConvert.SerializeObject(obj, new OptionalJsonConverter())!;
    }
}
