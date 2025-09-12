#:property TargetFramework=net10.0
#:property PublishAot=false

// The JSON would normally come from some external data source:
string json = "{}";

Foo foo = System.Text.Json.JsonSerializer.Deserialize<Foo>(json);

// This will print an empty string because C# will hydrate the model with the default value.
// There's no way to check if the model used a default value.
Console.WriteLine(foo.MyProperty); // ""    

public class Foo
{
    public string? MyProperty { get; set; }
}
