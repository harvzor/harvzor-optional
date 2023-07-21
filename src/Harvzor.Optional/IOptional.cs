using System;

namespace Harvzor.Optional;

/// <remarks>
/// Useful for when you want to check if an <see cref="Optional{T}"/> is being used without knowing what the type is.
/// </remarks>
/// <example>typeof(IOptional).IsAssignableFrom(typeToConvert)</example>
public interface IOptional
{
    /// <summary>
    /// Tells you if the underlying value has been defined.
    /// </summary>
    /// <example>
    /// var optionalString = new Optional&lt;string&gt;();
    /// <br/>
    /// // Will print "False":
    /// <br/>
    /// Console.WriteLine(optionalString.IsDefined);
    /// <br/>
    /// optionalString = "actual value";
    /// <br/>
    /// // Will print "True":
    /// <br/>
    /// Console.WriteLine(optionalString.IsDefined);
    /// </example>
    /// <remarks>
    /// It's sort of like in JS when you want to check if a value is defined with `myStr === undefined`.
    /// </remarks>
    bool IsDefined { get; }
    
    /// <summary>
    /// Allows you to access the actual underlying value.
    /// </summary>
    /// <example>
    /// var optionalString = new Optional&lt;string&gt;();
    /// <br/>
    /// // Prints the default value:
    /// <br/>
    /// Console.WriteLine(optionalString.Value);
    /// <br/>
    /// optionalString = "actual value";
    /// <br/>
    /// // Prints "actual value":
    /// <br/>
    /// Console.WriteLine(optionalString.Value);
    /// </example>
    /// <returns>
    /// Will return the default value of the underlying type if no value has been set.
    /// </returns>
    object Value { get; set; }
    
    /// <summary>
    /// Allows you to get the underlying type (<see cref="T"/>).
    /// </summary>
    Type GenericType { get; }
}
