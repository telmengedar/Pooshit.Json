using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Pooshit.Reflection;

namespace Pooshit.Json.Models;

/// <summary>
/// data model from source generator
/// </summary>
public class SourceGenModel : IModel {
    readonly Dictionary<string, IPropertyInfo> propertyCache = new();
    readonly Pooshit.Reflection.Model reflectedModel;

    readonly object accessLock = new();

    /// <summary>
    /// creates a new <see cref="Model"/>
    /// </summary>
    /// <param name="reflectedModel">model with reflected data</param>
    public SourceGenModel(Pooshit.Reflection.Model reflectedModel) => this.reflectedModel = reflectedModel;

    /// <inheritdoc />
    public IEnumerable<IPropertyInfo> Properties => reflectedModel.Properties;

    /// <inheritdoc />
    public IPropertyInfo GetProperty(string jsonName) {
        if (propertyCache.TryGetValue(jsonName, out IPropertyInfo property))
            return property;

        property = reflectedModel.Properties.FirstOrDefault(p => string.Compare(jsonName, p.Name, StringComparison.InvariantCultureIgnoreCase) == 0 || p.Attributes.Any(a => a is DataMemberAttribute dma && dma.Name == jsonName));
        lock(accessLock)
            return propertyCache[jsonName] = property;
    }
}