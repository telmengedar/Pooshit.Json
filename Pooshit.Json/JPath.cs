using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Pooshit.Json.Helpers;
using Pooshit.Json.Tokens;

namespace Pooshit.Json {
    
    /// <summary>
    /// class providing jpath functionality
    /// </summary>
    /// <remarks>
    /// this just behaves like jpath as needed and is not a proper jpath implementation
    /// </remarks>
    public static class JPath {

        /// <summary>
        /// retrieve values from a json structure
        /// </summary>
        /// <param name="data">json structure to select data from</param>
        /// <param name="path">path specifying data to select</param>
        /// <returns>selected data or null if no data matches path</returns>
        public static T Select<T>(object data, string path) {
            object value = Select(data, path);
            if (value is T typedvalue)
                return typedvalue;
            return (T)Converter.Convert(value, typeof(T), true);
        }

        static object SubSelect(object data, JPathToken[] token, int index) {
            if (data == null)
                return null;

            JPathToken currentToken = token[index];
            if (index == token.Length - 1) {
                if (currentToken.Index.HasValue) {
                    if (data is IEnumerable enumeration)
                        return enumeration.Cast<object>().Skip(currentToken.Index.Value).FirstOrDefault();
                    return null;
                }

                if (data is IDictionary dictionary)
                    return dictionary[currentToken.Property];
                if (data is IEnumerable properyEnumeration)
                    return properyEnumeration.Cast<object>().Where(i => i != null).Select(i => SubSelect(i, token, index));

                PropertyInfo property = data.GetType().GetProperty(currentToken.Property);
                if (property == null)
                    return null;
                return property.GetValue(data);
            }

            if (currentToken.Index.HasValue) {
                if (data is IEnumerable enumeration)
                    return SubSelect(enumeration.Cast<object>().Skip(currentToken.Index.Value).FirstOrDefault(), token, index + 1);
                return null;
            }

            if (data is IDictionary subDictionary)
                return SubSelect(subDictionary[currentToken.Property], token, index + 1);
            if (data is IEnumerable subProperyEnumeration) {
                return subProperyEnumeration.Cast<object>().Where(i => i != null)
                    .SelectMany(i => {
                        object result = SubSelect(i, token, index);
                        if (result is IEnumerable subenum)
                            return subenum.Cast<object>().ToArray();
                        return new[] {result};
                    });
            }

            PropertyInfo subProperty = data.GetType().GetProperty(currentToken.Property);
            if (subProperty == null)
                return null;
            return SubSelect(subProperty.GetValue(data), token, index + 1);
        } 
        
        /// <summary>
        /// retrieve values from a json structure
        /// </summary>
        /// <param name="data">json structure to select data from</param>
        /// <param name="path">path specifying element to select</param>
        /// <returns>result of path selection</returns>
        public static object Select(object data, string path) {
            JPathToken[] tokens = Parse(path).ToArray();
            return SubSelect(data, tokens, 0);
        }

        /// <summary>
        /// determines whether a data exists in a structure
        /// </summary>
        /// <param name="data">structure to analyse</param>
        /// <param name="path">path to property for which to check for</param>
        /// <returns>true if property exists, false otherwise</returns>
        public static bool Exists(object data, string path) {
            foreach (JPathToken token in Parse(path)) {
                if (data == null)
                    return false;

                if (token.Index.HasValue) {
                    if (data is IEnumerable enumeration)
                        data = enumeration.Cast<object>().Skip(token.Index.Value).FirstOrDefault();
                    else return false;
                }
                else {
                    if (data is IDictionary dictionary) {
                        if (!dictionary.Contains(token.Property))
                            return false;
                        data = dictionary[token.Property];
                    }
                    else {
                        PropertyInfo property = data.GetType().GetProperty(token.Property);
                        if (property == null)
                            return false;
                        data = property.GetValue(data);
                    }
                }
            }

            return data != null;
        }
        
        public static void Set(object data, string path, object value) {
            if (data == null)
                throw new ArgumentNullException(nameof(data), "base structure needs to be non null");
            
            JPathToken[] tokens = Parse(path).ToArray();
            for (int i = 0; i < tokens.Length - 1; ++i) {
                JPathToken token = tokens[i];
                
                if (token.Index.HasValue) {
                    if (data is Array array) {
                        if (token.Index.Value >= array.Length) {
                            ResizeArray(ref array, token.Index.Value + 1);
                            if (array.GetValue(token.Index.Value) == null) {
                                if (tokens[i + 1].Index.HasValue)
                                    array.SetValue(data = new List<object>(), token.Index.Value);
                                else array.SetValue(data = new Dictionary<string, object>(), token.Index.Value);
                            }
                        }
                    }
                    else if (data is IList list) {
                        while (token.Index.Value >= list.Count)
                            list.Add(null);
                        list[token.Index.Value] ??= tokens[i + 1].Index.HasValue ? (object)new List<object>() : new Dictionary<string, object>();
                    }
                    
                    if (data is IEnumerable enumeration) {
                        data = enumeration.Cast<object>().Skip(token.Index.Value).First();
                    }
                    else throw new InvalidOperationException("Indexer for non indexing property detected");
                }
                else {
                    if (data is IDictionary dictionary) {
                        if (!dictionary.Contains(token.Property)) {
                            if (tokens[i + 1].Index.HasValue)
                                dictionary[token.Property] = data = new List<object>();
                            else dictionary[token.Property] = data = new Dictionary<string, object>();
                        }
                        else data = dictionary[token.Property];
                    }
                    else {
                        PropertyInfo property = data.GetType().GetProperty(token.Property);
                        if (property == null)
                            throw new ArgumentException($"Property '{token.Property}' not found in object");
                        
                        data = property.GetValue(data);
                    }
                }
            }
            
            JPathToken hostToken = tokens.Last();
                
            if (hostToken.Index.HasValue) {
                if (data is Array array) {
                    if (hostToken.Index >= array.Length)
                        ResizeArray(ref array, hostToken.Index.Value + 1);
                    array.SetValue(value, hostToken.Index.Value);
                }
                else if (data is IList list) {
                    while (hostToken.Index >= list.Count)
                        list.Add(null);
                    list[hostToken.Index.Value] = value;
                }
                else throw new InvalidOperationException("Unable to access enumeration");
            }
            else {
                if (data is IDictionary dictionary) {
                    dictionary[hostToken.Property] = value;
                }
                else {
                    PropertyInfo property = data.GetType().GetProperty(hostToken.Property);
                    if (property == null)
                        throw new ArgumentException($"Property '{hostToken.Property}' not found in object");

                    property.SetValue(data, value);
                }
            }
        }

        static void ResizeArray(ref Array array, int n)
        {
            Type type = array.GetType();
            Type elemType = type.GetElementType();
            MethodInfo resizeMethod = typeof(Array).GetMethod("Resize", BindingFlags.Static | BindingFlags.Public);
            MethodInfo properResizeMethod = resizeMethod.MakeGenericMethod(elemType);
            object[] parameters = { array, n };
            properResizeMethod.Invoke(null, parameters);
            array = parameters[0] as Array;
        }
        
        public static IEnumerable<JPathToken> Parse(string path) {
            StringBuilder name = new StringBuilder();

            int parseMode = 0;
            foreach (char character in path) {
                switch (parseMode) {
                    case 0:
                        switch (character) {
                            case '/':
                                if (name.Length > 0) {
                                    yield return new JPathToken {
                                        Property = name.ToString()
                                    };
                                    name.Length = 0;
                                }

                                break;
                            case '[':
                                if (name.Length == 0)
                                    throw new InvalidOperationException("Indexer without property");
                                
                                yield return new JPathToken {
                                    Property = name.ToString()
                                };
                                name.Length = 0;
                                parseMode = 1;
                                break;
                            default:
                                name.Append(character);
                                break;
                        }
                        break;
                    case 1:
                        switch (character) {
                            case ']':
                                if (name.Length == 0)
                                    throw new InvalidCastException("No digits for indexer");

                                yield return new JPathToken {
                                    Index = int.Parse(name.ToString())
                                };
                                name.Length = 0;
                                parseMode = 0;
                                break;
                            default:
                                if (char.IsDigit(character))
                                    name.Append(character);
                                else throw new InvalidOperationException("Digit expected");
                                break;
                        }
                        break;
                }
            }

            if (name.Length > 0) {
                yield return new JPathToken {
                    Property = name.ToString()
                };
            }
        }
    }
}