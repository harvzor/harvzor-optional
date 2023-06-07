using System;
using System.Collections.Generic;
using System.Text;

namespace Harvzor.Optional
{
    public interface IOptional
    {
        bool IsDefined { get; }
        object Value { get; set; }
        Type GenericType { get; }
    }
}
