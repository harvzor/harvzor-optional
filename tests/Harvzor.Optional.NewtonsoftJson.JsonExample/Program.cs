using Harvzor.Optional;
using Harvzor.Optional.NewtonsoftJson;
using Newtonsoft.Json;

JsonSerializer serializer = new JsonSerializer();
serializer.Converters.Add(new OptionalJsonConverter());

// The JSON would normally come from some external data source:
string json = "{\"DefinedProperty\":\"Bar\"}";

Foo foo = JsonConvert.DeserializeObject<Foo>(json, new OptionalJsonConverter());

Console.WriteLine(foo.DefinedProperty.IsDefined); // True
Console.WriteLine(foo.UndefinedProperty.IsDefined); // False

// Now I can check if a value was defined before I try using it:
if (foo.DefinedProperty.IsDefined)
    Console.WriteLine(foo.DefinedProperty.Value); // "Bar" 
    
if (foo.UndefinedProperty.IsDefined)
    Console.WriteLine(foo.UndefinedProperty.Value); // Won't print as the value wasn't defined.

public class Foo
{
    public Optional<string> DefinedProperty { get; set; }
    public Optional<string> UndefinedProperty { get; set; }
}
