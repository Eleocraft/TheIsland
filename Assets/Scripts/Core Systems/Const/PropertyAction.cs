using System;
using System.Reflection;

public class PropertyAction<T>
{
    public readonly Action<T> Set;
    public readonly Func<T> Get;

    public PropertyAction(PropertyInfo propertyInfo)
    {
        Set = (Action<T>)Delegate.CreateDelegate(typeof(Action<T>), propertyInfo.GetSetMethod());
        Get = (Func<T>)Delegate.CreateDelegate(typeof(Func<T>), propertyInfo.GetGetMethod());
    }
}