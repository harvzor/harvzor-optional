using System.Reflection;

namespace Harvzor.Optional.NewtonsoftJson.Members;

internal class PropertyMember : IMember
{
    private readonly PropertyInfo _propertyInfo;

    public PropertyMember(PropertyInfo propertyInfo)
    {
        _propertyInfo = propertyInfo;
    }
    
    // public string Name => _propertyInfo.Name;
    //
    // public void SetValue(object obj, object value) => _propertyInfo.SetValue(obj, value, null);
    
    public object GetValue(object obj) => _propertyInfo.GetValue(obj, null);
    
    // public Type GetMemberType() => _propertyInfo.PropertyType;
    //
    // public MemberInfo GetMemberInfo() => _propertyInfo;
}