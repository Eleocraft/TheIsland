using UnityEngine;
using System;
using System.Reflection;
using System.Linq;

public class StaticObjectResetter : MonoBehaviour
{
    void OnDestroy()
    {
        FieldInfo[] fields = Assembly.GetExecutingAssembly().GetTypes()
                       .SelectMany(t => t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)) // Include instance methods in the search so we can show the developer an error instead of silently not adding instance methods to the dictionary
                       .Where(m => m.GetCustomAttributes(typeof(ResetOnDestroyAttribute), false).Length > 0)
                       .ToArray();
        
        foreach (FieldInfo field in fields)
        {
            if (!field.IsStatic)
                throw new Exception($"resetable fields should be static, but '{field.DeclaringType}.{field.Name}' is an instance field!");

            object newInstance = Activator.CreateInstance(field.FieldType);
            field.SetValue(newInstance, newInstance);
        }
    }
}
[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public class ResetOnDestroyAttribute : Attribute { }