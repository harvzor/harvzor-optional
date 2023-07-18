using System;
using System.Collections.Generic;
using System.Reflection;

namespace Harvzor.Optional.NewtonsoftJson;

interface IMember
{
    string Name { get; }
    
    void SetValue(object obj, object value);
    
    object GetValue(object obj);

    Type GetMemberType();

    MemberInfo GetMemberInfo();
}

class PropertyMember : IMember
{
    private readonly PropertyInfo _propertyInfo;

    public PropertyMember(PropertyInfo propertyInfo)
    {
        _propertyInfo = propertyInfo;
    }
    
    public string Name => _propertyInfo.Name;

    public void SetValue(object obj, object value) => _propertyInfo.SetValue(obj, value, null);
    
    public object GetValue(object obj) => _propertyInfo.GetValue(obj, null);
    
    public Type GetMemberType() => _propertyInfo.PropertyType;
    
    public MemberInfo GetMemberInfo() => _propertyInfo;
}

class FieldMember : IMember
{
    private readonly FieldInfo _fieldInfo;

    public FieldMember(FieldInfo fieldInfo)
    {
        _fieldInfo = fieldInfo;
    }

    public string Name => _fieldInfo.Name;

    public void SetValue(object obj, object value) => _fieldInfo.SetValue(obj, value);
    
    public object GetValue(object obj) => _fieldInfo.GetValue(obj);
    
    public Type GetMemberType() => _fieldInfo.FieldType;
    
    public MemberInfo GetMemberInfo() => _fieldInfo;
}

static class AssemblyExtension
{
    /// <remarks>
    /// https://stackoverflow.com/questions/12680341/how-to-get-both-fields-and-properties-in-single-call-via-reflection/1268045
    /// </remarks>
    public static IMember[] GetPropertiesAndFields(this Type t)
    {
        List<IMember> retList = new List<IMember>();

        foreach(PropertyInfo pi in t.GetProperties())
            retList.Add(new PropertyMember(pi));

        foreach(FieldInfo fi in t.GetFields())
            retList.Add(new FieldMember(fi));

        return retList.ToArray();
    }
}
