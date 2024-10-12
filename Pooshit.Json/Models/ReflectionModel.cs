using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Pooshit.Reflection;

namespace Pooshit.Json.Models;

/// <summary>
/// model using regular reflection
/// </summary>
public class ReflectionModel : IModel {
    readonly Dictionary<string, IPropertyInfo> propertyCache = new();
    readonly object accessLock = new();
    readonly Type modelType;

    /// <summary>
    /// creates a new <see cref="ReflectionModel"/>
    /// </summary>
    /// <param name="modelType">type of model of which to reflect info</param>
    public ReflectionModel(Type modelType) => this.modelType = modelType;

    /// <inheritdoc />
    public IEnumerable<IPropertyInfo> Properties => modelType.GetProperties().Select(p => new ReflectionProperty(p));

    /// <inheritdoc />
    public IPropertyInfo GetProperty(string jsonName) {
        if (propertyCache.TryGetValue(jsonName, out IPropertyInfo property))
            return property;

        PropertyInfo reflectionProperty = modelType.GetProperties().FirstOrDefault(p => p.GetIndexParameters().Length==0 && string.Compare(jsonName, p.Name, StringComparison.InvariantCultureIgnoreCase) == 0 || p.GetCustomAttributes().Any(a => a is DataMemberAttribute dma && dma.Name == jsonName));
        property = reflectionProperty != null ? new ReflectionProperty(reflectionProperty) : null;
        lock(accessLock)
            return propertyCache[jsonName] = property;
    }
}