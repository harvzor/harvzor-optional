using System.Reflection;

namespace Harvzor.Optional.NewtonsoftJson.Members;

internal class FieldMember : IMember
{
    private readonly FieldInfo _fieldInfo;

    public FieldMember(FieldInfo fieldInfo)
    {
        _fieldInfo = fieldInfo;
    }

    // public string Name => _fieldInfo.Name;
    //
    // public void SetValue(object obj, object value) => _fieldInfo.SetValue(obj, value);
    
    public object GetValue(object obj) => _fieldInfo.GetValue(obj);
    
    // public Type GetMemberType() => _fieldInfo.FieldType;
    //
    // public MemberInfo GetMemberInfo() => _fieldInfo;
}