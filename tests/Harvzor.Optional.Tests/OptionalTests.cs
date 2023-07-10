namespace Harvzor.Optional.Tests;

public class OptionalTests
{
    [Fact]
    public void IsDefined_ShouldBeFalse_WhenNotDefined()
    {
        // Arrange

        Optional<string> optionalString = new();
        
        // Assert

        optionalString.IsDefined.ShouldBe(false);
    }
    
    [Fact]
    public void IsDefined_ShouldBeTrue_WhenImplicitlySet()
    {
        // Arrange

        Optional<string> optionalString = "some value";
        
        // Assert

        optionalString.IsDefined.ShouldBe(true);
    }
    
    [Fact]
    public void IsDefined_ShouldBeTrue_WhenSetOnValueProperty()
    {
        // Arrange

        Optional<string> optionalString = new()
        {
            Value = "some value"
        };

        // Assert

        optionalString.IsDefined.ShouldBe(true);
    }
    
    [Fact]
    public void Value_ShouldBeCorrect_WhenImplicitlySet()
    {
        // Arrange

        Optional<string> optionalString = "some value";

        // Assert

        optionalString.Value.ShouldBe("some value");
    }
    
    [Fact]
    public void Value_ShouldBeCorrect_WhenSetOnValueProperty()
    {
        // Arrange

        Optional<string> optionalString = new()
        {
            Value = "some value"
        };

        // Assert

        optionalString.Value.ShouldBe("some value");
    }
    
    [Fact]
    public void Value_ShouldBeDefault_WhenNotDefined()
    {
        // Arrange

        Optional<string> optionalString = new();
        
        // Assert

        optionalString.Value.ShouldBe(default);
    }

    [Fact]
    public void ExplicitOperator_ShouldBeCorrect_WhenSetOnValueProperty()
    {
        // Arrange

        Optional<string> optionalString = new()
        {
            Value = "some value"
        };
        
        // Assert

        ((string)optionalString).ShouldBe("some value");
    }
}
