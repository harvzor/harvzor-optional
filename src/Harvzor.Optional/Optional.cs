using System;

namespace Harvzor.Optional;

/// <summary>
/// Use when you want to know if something has been explicitly instantiated.
/// </summary>
public struct Optional<T> : IOptional
{
    private object _value;

    public Optional(T value) : this()
    {
        Value = value;
    }

    /// <inheritdoc cref="IOptional.IsDefined"/>
    public bool IsDefined { get; private set; }

    /// <inheritdoc cref="IOptional.Value"/>
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

    /// <inheritdoc cref="IOptional.GenericType"/>
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
        return new Optional<T>(value: value);
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
