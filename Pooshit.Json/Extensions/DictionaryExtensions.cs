using System;
using System.Collections.Generic;
using Pooshit.Json.Models;
using Pooshit.Reflection;
using Model = Pooshit.Json.Models.Model;

namespace Pooshit.Json.Extensions;

/// <summary>
/// extension methods for dictionaries
/// </summary>
public static class DictionaryExtensions {
        
    /// <summary>
    /// reads a type from dictionary values
    /// </summary>
    /// <param name="dictionary">dictionary providing values</param>
    /// <param name="type">type to read</param>
    /// <param name="errorHandler">handler used to read value if automatic conversion fails (optional)</param>
    /// <returns>instantiated type filled with values from dictionary</returns>
    public static object ReadType(this IDictionary<string, object> dictionary, Type type, Func<Exception, Type, object, object> errorHandler=null) {
        object host = Activator.CreateInstance(type);
        IModel typemodel = Model.GetModel(type);
        foreach (KeyValuePair<string, object> kvp in dictionary) {
            IPropertyInfo property = typemodel.GetProperty(kvp.Key);
            if (property == null)
                continue;
            if (property.PropertyType.IsArray)
                property.SetValue(host, kvp.Value.ReadValueAsArray(property.PropertyType.GetElementType(), errorHandler));
            else property.SetValue(host, kvp.Value.ReadStructureValue(property.PropertyType, errorHandler));
        }

        return host;
    }
}