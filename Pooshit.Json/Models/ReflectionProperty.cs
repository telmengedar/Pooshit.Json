using System;
using System.Linq;
using System.Reflection;
using Pooshit.Reflection;

namespace Pooshit.Json.Models;

/// <summary>
/// property info using system reflection to provide info
/// </summary>
public class ReflectionProperty : IPropertyInfo {
    readonly PropertyInfo propertyInfo;

    /// <summary>
    /// creates a new <see cref="ReflectionProperty"/>
    /// </summary>
    /// <param name="propertyInfo">property of system reflection</param>
    public ReflectionProperty(PropertyInfo propertyInfo) => this.propertyInfo = propertyInfo;

    /// <inheritdoc />
    public object GetValue(object instance) => propertyInfo.GetValue(instance);

    /// <inheritdoc />
    public void SetValue(object instance, object value) {
        propertyInfo.SetValue(instance, value);
    }

    /// <inheritdoc />
    public string Name => propertyInfo.Name;

    /// <inheritdoc />
    public Attribute[] Attributes => propertyInfo.GetCustomAttributes().ToArray();

    /// <inheritdoc />
    public Type PropertyType => propertyInfo.PropertyType;

    /// <inheritdoc />
    public bool HasGetter => propertyInfo.CanRead;

    /// <inheritdoc />
    public bool HasSetter => propertyInfo.CanWrite;
}