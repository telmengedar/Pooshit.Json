using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Pooshit.Json.Helpers;
using Pooshit.Json.Tokens;

namespace Pooshit.Json;

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
        return Select<T>(data, path, false);
    }

    /// <summary>
    /// retrieve values from a json structure
    /// </summary>
    /// <param name="data">json structure to select data from</param>
    /// <param name="path">path specifying data to select</param>
    /// <param name="ignoreCase">determines whether to ignore casing when checking path</param>
    /// <returns>selected data or null if no data matches path</returns>
    public static T Select<T>(object data, string path, bool ignoreCase) {
        object value = Select(data, path, ignoreCase);
        if (value is T typedvalue)
            return typedvalue;
        return (T)Converter.Convert(value, typeof(T), true);
    }

    static object SubSelect(object data, JPathToken[] token, int index, bool ignoreCase) {
        if (data == null)
            return null;

        BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;
        if (ignoreCase) flags |= BindingFlags.IgnoreCase;

        JPathToken currentToken = token[index];
        if (index == token.Length - 1) {
            if (currentToken.Index.HasValue) {
                if (data is IEnumerable enumeration)
                    return enumeration.Cast<object>().Skip(currentToken.Index.Value).FirstOrDefault();
                return null;
            }

            if (data is IDictionary dictionary) {
                if (ignoreCase) {
                    object key = dictionary.Keys.Cast<object>()
                                           .FirstOrDefault(k => string.Compare(k?.ToString(), currentToken.Property, StringComparison.InvariantCultureIgnoreCase) == 0);
                    return key == null ? null : dictionary[key];
                }
                return dictionary[currentToken.Property];
            }

            if (data is IEnumerable properyEnumeration)
                return properyEnumeration.Cast<object>()
                                         .Where(i => i != null)
                                         .Select(i => SubSelect(i, token, index, ignoreCase));

            PropertyInfo property = data.GetType().GetProperty(currentToken.Property, flags);
            if (property == null)
                return null;
            return property.GetValue(data);
        }

        if (currentToken.Index.HasValue) {
            if (data is IEnumerable enumeration)
                return SubSelect(enumeration.Cast<object>().Skip(currentToken.Index.Value).FirstOrDefault(), token, index + 1, ignoreCase);
            return null;
        }

        if (data is IDictionary subDictionary) {
            if (ignoreCase) {
                object key = subDictionary.Keys.Cast<object>()
                                       .FirstOrDefault(k => string.Compare(k?.ToString(), currentToken.Property, StringComparison.InvariantCultureIgnoreCase) == 0);
                if(key==null)
                    return null;
                return SubSelect(subDictionary[key], token, index + 1, true);            
            }

            return SubSelect(subDictionary[currentToken.Property], token, index + 1, false);
        }

        if (data is IEnumerable subProperyEnumeration) {
            return subProperyEnumeration.Cast<object>().Where(i => i != null)
                                        .SelectMany(i => {
                                            object result = SubSelect(i, token, index, ignoreCase);
                                            if (result is IEnumerable subenum)
                                                return subenum.Cast<object>().ToArray();
                                            return [result];
                                        });
        }
        
        PropertyInfo subProperty = data.GetType()
                                       .GetProperty(currentToken.Property, flags);
        if (subProperty == null)
            return null;
        return SubSelect(subProperty.GetValue(data), token, index + 1, ignoreCase);
    } 
        
    /// <summary>
    /// retrieve values from a json structure
    /// </summary>
    /// <param name="data">json structure to select data from</param>
    /// <param name="path">path specifying element to select</param>
    /// <returns>result of path selection</returns>
    public static object Select(object data, string path) {
        JPathToken[] tokens = Parse(path).ToArray();
        return SubSelect(data, tokens, 0, false);
    }

    /// <summary>
    /// retrieve values from a json structure
    /// </summary>
    /// <param name="data">json structure to select data from</param>
    /// <param name="path">path specifying element to select</param>
    /// <param name="ignoreCase">determines whether to ignore property casing</param>
    /// <returns>result of path selection</returns>
    public static object Select(object data, string path, bool ignoreCase) {
        JPathToken[] tokens = Parse(path).ToArray();
        return SubSelect(data, tokens, 0, ignoreCase);
    }

    /// <summary>
    /// determines whether a data exists in a structure
    /// </summary>
    /// <param name="data">structure to analyse</param>
    /// <param name="path">path to property for which to check for</param>
    /// <returns>true if property exists, false otherwise</returns>
    public static bool Exists(object data, string path) {
        return Exists(data, path, false);
    }

    /// <summary>
    /// determines whether a data exists in a structure
    /// </summary>
    /// <param name="data">structure to analyse</param>
    /// <param name="path">path to property for which to check for</param>
    /// <param name="ignoreCase">determines whether to igore casing when checking for properties</param>
    /// <returns>true if property exists, false otherwise</returns>
    public static bool Exists(object data, string path, bool ignoreCase) {
        BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;
        if (ignoreCase) flags |= BindingFlags.IgnoreCase;
        
        foreach (JPathToken token in Parse(path)) {
            if (data == null)
                return false;

            if (token.Index.HasValue) {
                if (data is IEnumerable enumeration)
                    data = enumeration.Cast<object>()
                                      .Skip(token.Index.Value)
                                      .FirstOrDefault();
                else return false;
            }
            else {
                if (data is IDictionary dictionary) {
                    if (ignoreCase) {
                        object key = dictionary.Keys.Cast<object>()
                                               .FirstOrDefault(k => string.Compare(k?.ToString(), token.Property, StringComparison.InvariantCultureIgnoreCase) == 0);
                        if (key == null)
                            return false;
                        
                        data = dictionary[key];
                    }
                    else {
                        if (!dictionary.Contains(token.Property))
                            return false;
                        data = dictionary[token.Property];
                    }
                }
                else {
                    PropertyInfo property = data.GetType()
                                                .GetProperty(token.Property, flags);
                    if (property == null)
                        return false;
                    data = property.GetValue(data);
                }
            }
        }

        return data != null;
    }

    /// <summary>
    /// set a value to a structure
    /// </summary>
    /// <param name="data">host structure</param>
    /// <param name="path">path where to set value</param>
    /// <param name="value">value to set</param>
    public static void Set(object data, string path, object value) {
        Set(data, path, value, false);
    }

    /// <summary>
    /// set a value to a structure
    /// </summary>
    /// <param name="data">host structure</param>
    /// <param name="path">path where to set value</param>
    /// <param name="value">value to set</param>
    /// <param name="ignoreCase">determines whether to ignore casing when setting existing properties</param>
    public static void Set(object data, string path, object value, bool ignoreCase) {
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
                    list[token.Index.Value] ??= tokens[i + 1].Index.HasValue ? new List<object>() : new Dictionary<string, object>();
                }
                    
                if (data is IEnumerable enumeration) {
                    data = enumeration.Cast<object>().Skip(token.Index.Value).First();
                }
                else throw new InvalidOperationException("Indexer for non indexing property detected");
            }
            else {
                if (data is IDictionary dictionary) {
                    if (ignoreCase) {
                        object key = dictionary.Keys
                                               .Cast<object>()
                                               .FirstOrDefault(k => string.Compare(k?.ToString(), token.Property, StringComparison.InvariantCultureIgnoreCase) == 0);

                        if (key == null) {
                            if (tokens[i + 1].Index.HasValue)
                                dictionary[token.Property] = data = new List<object>();
                            else dictionary[token.Property] = data = new Dictionary<string, object>();
                        }
                        else {
                            if (tokens[i + 1].Index.HasValue)
                                dictionary[key] = data = new List<object>();
                            else dictionary[key] = data = new Dictionary<string, object>();
                        }
                    }
                    else {
                        if (!dictionary.Contains(token.Property)) {
                            if (tokens[i + 1].Index.HasValue)
                                dictionary[token.Property] = data = new List<object>();
                            else dictionary[token.Property] = data = new Dictionary<string, object>();
                        }
                        else data = dictionary[token.Property];
                    }
                }
                else {
                    BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;
                    if(ignoreCase) flags |= BindingFlags.IgnoreCase;
                    
                    PropertyInfo property = data.GetType().GetProperty(token.Property, flags);
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
                if (ignoreCase) {
                    object key = dictionary.Keys
                                           .Cast<object>()
                                           .FirstOrDefault(k => string.Compare(k?.ToString(), hostToken.Property, StringComparison.InvariantCultureIgnoreCase) == 0);

                    if (key == null)
                        dictionary[hostToken.Property] = data;
                    else dictionary[key] = data;
                }
                else dictionary[hostToken.Property] = value;
            }
            else {
                BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;
                if(ignoreCase) flags |= BindingFlags.IgnoreCase;

                PropertyInfo property = data.GetType().GetProperty(hostToken.Property, flags);
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
        
    /// <summary>
    /// parses a jpath
    /// </summary>
    /// <param name="path">path data to parse</param>
    /// <returns>tokens parsed from path</returns>
    public static IEnumerable<JPathToken> Parse(string path) {
        StringBuilder name = new();

        int parseMode = 0;
        foreach (char character in path) {
            switch (parseMode) {
                case 0:
                    switch (character) {
                        case '/':
                            if (name.Length > 0) {
                                yield return new() {
                                    Property = name.ToString()
                                };
                                name.Length = 0;
                            }

                        break;
                        case '[':
                            if (name.Length != 0) {
                                yield return new() {
                                    Property = name.ToString()
                                };
                                name.Length = 0;
                            }

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

                            yield return new() {
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
            yield return new() {
                Property = name.ToString()
            };
        }
    }
}