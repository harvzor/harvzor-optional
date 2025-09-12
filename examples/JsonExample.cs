#:property TargetFramework=net10.0
#:property PublishAot=false

#:package Harvzor.Optional@0.2.0
#:package Harvzor.Optional.SystemTextJson@0.2.0

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Harvzor.Optional.SystemTextJson;
using Harvzor.Optional;

JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions();
jsonSerializerOptions.Converters.Add(new Harvzor.Optional.SystemTextJson.OptionalJsonConverter());
jsonSerializerOptions.TypeInfoResolver = new DefaultJsonTypeInfoResolver
{
    Modifiers = { OptionalTypeInfoResolverModifiers.IgnoreUndefinedOptionals }
};

// The JSON would normally come from some external data source:
string json = "{\"DefinedProperty\":\"Bar\"}";

Foo foo = JsonSerializer.Deserialize<Foo>(json, jsonSerializerOptions)!;

Console.WriteLine(foo.DefinedProperty.IsDefined); // True
Console.WriteLine(foo.UndefinedProperty.IsDefined); // False

// Now I can check if a value was defined before I try using it:
if (foo.DefinedProperty.IsDefined) // True
    Console.WriteLine(foo.DefinedProperty.Value); // "Bar" 
    
if (foo.UndefinedProperty.IsDefined) // False
    Console.WriteLine(foo.UndefinedProperty.Value); // Won't print as the value wasn't defined.

public class Foo
{
    public Optional<string> DefinedProperty { get; set; }
    public Optional<string> UndefinedProperty { get; set; }
}
