using System;
using System.Reflection;

namespace Harvzor.Optional.NewtonsoftJson.Members;

internal static class MemberExtensions
{
    /// <summary>
    /// Finds the property or field on a type, and returns back an <see cref="IMember"/> which simplifies accessing
    /// similar methods on those properties/fields.
    /// </summary>
    /// <remarks>
    /// https://stackoverflow.com/questions/12680341/how-to-get-both-fields-and-properties-in-single-call-via-reflection/1268045
    /// </remarks>
    public static IMember? GetPropertyOrField(this Type t, string propertyOrFieldName)
    {
        PropertyInfo? property = t.GetProperty(propertyOrFieldName);

        if (property != null)
            return new PropertyMember(property);
        
        FieldInfo field = t.GetField(propertyOrFieldName);

        if (field != null)
            return new FieldMember(field);

        return null;
    }
}
