using System;

namespace Harvzor.Optional;

/// <remarks>
/// Pretty sure this is necessary because in my custom JSON converters, I don't know what the T type is.
/// </remarks>
public interface IOptional
{
    bool IsDefined { get; }
    object Value { get; set; }
    Type GenericType { get; }
}
