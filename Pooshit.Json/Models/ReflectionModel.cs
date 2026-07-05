using System;
using System.Collections.Concurrent;
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
    readonly ConcurrentDictionary<string, IPropertyInfo> propertyCache = new();
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
        return propertyCache.GetOrAdd(jsonName, name => {
            PropertyInfo reflectionProperty = modelType.GetProperties().FirstOrDefault(p => p.GetIndexParameters().Length == 0 && string.Compare(name, p.Name, StringComparison.InvariantCultureIgnoreCase) == 0 || p.GetCustomAttributes().Any(a => a is DataMemberAttribute dma && dma.Name == name));
            return reflectionProperty != null ? new ReflectionProperty(reflectionProperty) : null;
        });
    }
}