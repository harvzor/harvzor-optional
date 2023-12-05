namespace Harvzor.Optional.NewtonsoftJson.Members;

/// <summary>
/// Allows you to treat properties/fields on an object the same.
/// </summary>
internal interface IMember
{
    // string Name { get; }
    //
    // void SetValue(object obj, object value);
    
    /// <summary>
    /// Returns the value of a member supported by a given object.
    /// </summary>
    /// <param name="obj">The object whose member value will be returned.</param>
    /// <returns>An object containing the value of the member reflected by this instance.</returns>
    object GetValue(object obj);

    // Type GetMemberType();
    //
    // MemberInfo GetMemberInfo();
}