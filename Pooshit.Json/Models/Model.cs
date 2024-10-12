using System;
using System.Collections.Generic;

namespace Pooshit.Json.Models;

/// <summary>
/// json model used to reflect on properties to serialize/deserialize
/// </summary>
public class Model {
    static readonly Dictionary<Type, IModel> modelCache = new();
    static readonly object modelLock = new();


    /// <summary>
    /// get json data model for the specified type
    /// </summary>
    /// <param name="dataType">type of data of which to get model</param>
    /// <returns>json data model</returns>
    public static IModel GetModel(Type dataType) {
        lock (modelLock) {
            if (!modelCache.TryGetValue(dataType, out IModel model)) {
                try {
                    modelCache[dataType] = model = new SourceGenModel(Pooshit.Reflection.Reflection.GetModel(dataType));
                }
                catch (Exception) {
                    modelCache[dataType] = model = new ReflectionModel(dataType);
                }
            }

            return model;
        }
    }

    /// <summary>
    /// get json data model for the specified type
    /// </summary>
    /// <typeparam name="T">type of data of which to get model</typeparam>
    /// <returns>json data model</returns>
    public static IModel GetModel<T>() => GetModel(typeof(T));
}