using System.Collections.Generic;
using Pooshit.Reflection;

namespace Pooshit.Json.Models;

/// <summary>
/// json model used to reflect on properties to serialize/deserialize
/// </summary>
public interface IModel {

    /// <summary>
    /// properties in model
    /// </summary>
    IEnumerable<IPropertyInfo> Properties { get; }
    
    /// <summary>
    /// get property info from model
    /// </summary>
    /// <param name="jsonName">name of property to get</param>
    /// <returns>property info for property with the specified name</returns>
    IPropertyInfo GetProperty(string jsonName);
}