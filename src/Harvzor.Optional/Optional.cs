using System;

namespace Harvzor.Optional;

/// <summary>
/// Use when you want to know if something has been explicitly instantiated.
/// </summary>
public struct Optional<T> : IOptional
{
    private object _value;

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
    public bool IsDefined { get; private set; }

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
    public T Value
    {
        get => IsDefined ? (T)_value : default;
        // ReSharper disable once MemberCanBePrivate.Global
        set
        {
            _value = value;
            IsDefined = true;
        }
    }

    /// <summary>
    /// Allows you to get the underlying type (<see cref="T"/>).
    /// </summary>
    public Type GenericType => typeof(T);

    object IOptional.Value
    {
        get => Value;
        set
        {
            IsDefined = true;
            _value = value;
        }
    }

    /// <summary>
    /// Allows you to treat this object as if it's the underlying type when setting values, rather than needing to access the <see cref="Value"/> property.
    /// </summary>
    /// <example>
    /// var optionalString = new Optional&lt;string&gt;();
    /// <br/>
    /// // You can write this:
    /// <br/>
    /// optionalString = "actual value";
    /// <br/>
    /// // Instead of this:
    /// <br/>
    /// optionalString.Value = "actual value";
    /// </example>
    public static implicit operator Optional<T>(T value)
    {
        return new Optional<T> { Value = value };
    }

    /// <summary>
    /// Allows you to cast back to the underlying type.
    /// </summary>
    /// <example>
    /// var optionalString = new Optional&lt;string&gt;();
    /// <br/>
    /// // You can write this:
    /// <br/>
    /// Console.WriteLine((string)optionalString);
    /// <br/>
    /// // Instead of this:
    /// <br/>
    /// Console.WriteLine(optionalString.Value);
    /// </example>
    public static explicit operator T(Optional<T> optional)
    {
        return optional.Value;
    }
}
