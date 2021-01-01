using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

namespace NightlyCode.Json.Models {
    
    /// <summary>
    /// model used to retrieve properties when reading json to types
    /// </summary>
    public class Model {
        static readonly Dictionary<Type, Model> models=new Dictionary<Type, Model>();
        readonly Dictionary<string, PropertyInfo> properties=new Dictionary<string, PropertyInfo>();
        static readonly object cachelock=new object();
        object modellock = new object();
        
        /// <summary>
        /// gets or creates a model for the specified type
        /// </summary>
        /// <param name="type">type of which to get model</param>
        /// <returns>model for type</returns>
        public static Model Get(Type type) {
            Model model;
            lock (cachelock) {
                if (!models.TryGetValue(type, out model)) {
                    models[type] = model = new Model();


                    foreach (PropertyInfo property in type.GetProperties()) {
                        DataMemberAttribute datamember = (DataMemberAttribute) Attribute.GetCustomAttribute(property, typeof(DataMemberAttribute));
                        if (datamember != null)
                            model.properties[datamember.Name] = property;
                        else model.properties[property.Name.ToLower()] = property;

                        model.properties[property.Name] = property;
                    }
                }
            }

            return model;
        }

        /// <summary>
        /// get property from key
        /// </summary>
        /// <param name="key">key to analyse</param>
        /// <returns>property stored under key</returns>
        public PropertyInfo GetProperty(string key) {
            PropertyInfo property;
            lock (modellock) {
                if (properties.TryGetValue(key, out property)) return property;
                
                properties.TryGetValue(key.ToLower(), out property);
                properties[key] = property;
            }

            return property;
        }
    }
}