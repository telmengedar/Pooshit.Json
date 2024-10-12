using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pooshit.Json.Helpers;
using Pooshit.Json.Models;
using Pooshit.Json.Tokens;
using Pooshit.Reflection;
using IDictionary = System.Collections.IDictionary;
using Model = Pooshit.Json.Models.Model;

namespace Pooshit.Json.Reader;

/// <inheritdoc />
public class JsonReader : IJsonReader {
    const char eof = '\0';
    readonly StringBuilder buffer=new();
    readonly char[] unicodebuffer=new char[4];

    /// <inheritdoc />
    public object Read(Type type, IDataReader reader) {
        char state = eof;
        return Converter.Convert(Read(type, reader, ref state), type);
    }

    public async Task<object> ReadAsync(Type type, IDataReader reader) {
        AsyncState state = new() {
                                     State = eof
                                 };

        return Converter.Convert(await ReadAsync(type, reader, state), type);
    }

    object Read(Type type, IDataReader reader, ref char state) {
        state = reader.ReadCharacter();
        while (char.IsWhiteSpace(state))
            state = reader.ReadCharacter();

        if (state == eof)
            return null;
            
        switch (state) {
            case '{':
                return ReadObject(type, reader, ref state);
            case '[':
                if (type == typeof(object))
                    return ReadArray(typeof(object), reader, ref state);
                if (!type.IsArray)
                    throw new FormatException("Trying to read array value to non array result type");
                return ReadArray(type.GetElementType(), reader, ref state);
            case '\"':
                return ReadString(reader, ref state);
            case ']':
            case '}':
                return new NoData();
            default:
                return ReadValue(state, type, reader, ref state);
        }
    }

    async Task<object> ReadAsync(Type type, IDataReader reader, AsyncState state) {
        state.State = await reader.ReadCharacterAsync();
        while (char.IsWhiteSpace(state.State))
            state.State = await reader.ReadCharacterAsync();

        if (state.State == eof)
            return null;
            
        switch (state.State) {
            case '{':
                return await ReadObjectAsync(type, reader, state);
            case '[':
                if (type == typeof(object))
                    return await ReadArrayAsync(typeof(object), reader, state);
                if (!type.IsArray)
                    throw new FormatException("Trying to read array value to non array result type");
                return await ReadArrayAsync(type.GetElementType(), reader, state);
            case '\"':
                return await ReadStringAsync(reader, state);
            case ']':
            case '}':
                return new NoData();
            default:
                return await ReadValueAsync(type, reader, state);
        }
    }

    object ReadObject(Type type, IDataReader reader, ref char state) {
        if (type == typeof(object) || typeof(IDictionary).IsAssignableFrom(type) || (type.IsGenericType && type.GetGenericTypeDefinition()==typeof(IDictionary<,>)))  {
            if (type.IsGenericType && !typeof(IDictionary<string, object>).IsAssignableFrom(type)) {
                Type[] arguments = type.GetGenericArguments();
                IDictionary dictionary = (IDictionary)Activator.CreateInstance(type);
                
                do {
                    object key = Converter.Convert(Read(typeof(string), reader, ref state), arguments[0]);
                    if (key is NoData)
                        break;

                    if (state != ':') {
                        state = reader.ReadCharacter();
                        while (char.IsWhiteSpace(state))
                            state = reader.ReadCharacter();
                    }

                    if (state != ':')
                        throw new FormatException("Missing ':' in json dictionary");

                    object value = Converter.Convert(Read(typeof(object), reader, ref state), arguments[1]);
                    
                    dictionary[key] = value;

                    if (state != '}' && state != ',') {
                        state = reader.ReadCharacter();
                        while (char.IsWhiteSpace(state))
                            state = reader.ReadCharacter();
                    }
                } while (state != eof && state != '}');

                state = eof;
                return dictionary;

            }
            else {
                Dictionary<string, object> dictionary = new();

                do {
                    object key = Read(typeof(string), reader, ref state);
                    if (key is NoData)
                        break;

                    if (state != ':') {
                        state = reader.ReadCharacter();
                        while (char.IsWhiteSpace(state))
                            state = reader.ReadCharacter();
                    }

                    if (state != ':')
                        throw new FormatException("Missing ':' in json dictionary");

                    object value = Read(typeof(object), reader, ref state);

                    dictionary[key.ToString()] = value;

                    if (state != '}' && state != ',') {
                        state = reader.ReadCharacter();
                        while (char.IsWhiteSpace(state))
                            state = reader.ReadCharacter();
                    }
                } while (state != eof && state != '}');

                state = eof;
                return dictionary;
            }
        }

        IModel model = Model.GetModel(type);
        object result = Activator.CreateInstance(type);
            
        do {
            object key = Read(typeof(string), reader, ref state);
            if (key is NoData)
                break;
                
            if (state != ':') {
                state=reader.ReadCharacter();
                while (char.IsWhiteSpace(state))
                    state = reader.ReadCharacter();
            }

            if (state != ':')
                throw new FormatException("Missing ':' in json dictionary");

            IPropertyInfo property = model.GetProperty(key.ToString());
            if (property == null) {
                // read value and skip property
                Read(typeof(object), reader, ref state);
            }
            else{
                object value = Read(property.PropertyType, reader, ref state);
                if (value != null && value.GetType() != property.PropertyType)
                    value = Converter.Convert(value, property.PropertyType);
                property.SetValue(result, value);
            }

            if (state != '}' && state != ',') {
                state=reader.ReadCharacter();
                while (char.IsWhiteSpace(state))
                    state = reader.ReadCharacter();
            }
        } while (state != eof && state!='}');

        state = eof;
        return result;
    }

    async Task<object> ReadObjectAsync(Type type, IDataReader reader, AsyncState state) {
        if (type == typeof(object) || type == typeof(IDictionary) || typeof(IDictionary<string,object>).IsAssignableFrom(type)) {
            Dictionary<string, object> dictionary = new();

            do {
                object key = await ReadAsync(typeof(string), reader, state);
                if (key is NoData)
                    break;
                    
                if (state.State != ':') {
                    state.State=await reader.ReadCharacterAsync();
                    while (char.IsWhiteSpace(state.State))
                        state.State = await reader.ReadCharacterAsync();
                }

                if (state.State != ':')
                    throw new FormatException("Missing ':' in json dictionary");

                object value = await ReadAsync(typeof(object), reader, state);

                dictionary[key.ToString()] = value;
                    
                if (state.State != '}' && state.State != ',') {
                    state.State=await reader.ReadCharacterAsync();
                    while (char.IsWhiteSpace(state.State))
                        state.State = await reader.ReadCharacterAsync();
                }
            } while (state.State != eof && state.State!='}');

            state.State = eof;
            return dictionary;
        }

        IModel model = Model.GetModel(type);
        object result = Activator.CreateInstance(type);
            
        do {
            object key = await ReadAsync(typeof(string), reader, state);
            if (key is NoData)
                break;
                
            if (state.State != ':') {
                state.State=await reader.ReadCharacterAsync();
                while (char.IsWhiteSpace(state.State))
                    state.State = await reader.ReadCharacterAsync();
            }

            if (state.State != ':')
                throw new FormatException("Missing ':' in json dictionary");

            IPropertyInfo property = model.GetProperty(key.ToString());
            if (property == null) {
                // read value and skip property
                await ReadAsync(typeof(object), reader, state);
            }
            else{
                object value = await ReadAsync(property.PropertyType, reader, state);
                if (value != null && value.GetType() != property.PropertyType)
                    value = Converter.Convert(value, property.PropertyType);
                property.SetValue(result, value);
            }

            if (state.State != '}' && state.State != ',') {
                state.State=await reader.ReadCharacterAsync();
                while (char.IsWhiteSpace(state.State))
                    state.State = await reader.ReadCharacterAsync();
            }
        } while (state.State != eof && state.State!='}');

        state.State = eof;
        return result;
    }

    Array ReadArray(Type elementtype, IDataReader reader, ref char state) {
        List<object> items=new();
        do {
            object item = Read(elementtype, reader, ref state);
            if (item is NoData)
                break;
                
            items.Add(item);
            if (state != ',' && state != ']') {
                state = reader.ReadCharacter();
                while (char.IsWhiteSpace(state))
                    state = reader.ReadCharacter();
            }
        } while (state != eof && state!=']');

        Array result = Array.CreateInstance(elementtype, items.Count);
        for (int i = 0; i < items.Count; ++i) {
            object item = items[i];
            if (item != null && item.GetType() != elementtype)
                item = Converter.Convert(item, elementtype);
            result.SetValue(item, i);
        }

        state = eof;
        return result;
    }

    async Task<Array> ReadArrayAsync(Type elementtype, IDataReader reader, AsyncState state) {
        List<object> items=new();
        do {
            object item = await ReadAsync(elementtype, reader, state);
            if (item is NoData)
                break;
                
            items.Add(item);
            if (state.State != ',' && state.State != ']') {
                state.State = await reader.ReadCharacterAsync();
                while (char.IsWhiteSpace(state.State))
                    state.State = await reader.ReadCharacterAsync();
            }
        } while (state.State != eof && state.State!=']');

        Array result = Array.CreateInstance(elementtype, items.Count);
        for (int i = 0; i < items.Count; ++i) {
            object item = items[i];
            if (item != null && item.GetType() != elementtype)
                item = Converter.Convert(item, elementtype);
            result.SetValue(item, i);
        }

        state.State = eof;
        return result;
    }

    string ReadString(IDataReader reader, ref char state) {
        buffer.Length = 0;
        do {
            state = reader.ReadCharacter();
            switch (state) {
                case '"':
                    break;
                case '\\':
                    state = reader.ReadCharacter();
                    switch (state) {
                        case eof:
                            break;
                        case 't':
                            buffer.Append('\t');
                            break;
                        case 'r':
                            buffer.Append('\r');
                            break;
                        case 'n':
                            buffer.Append('\n');
                            break;
                        case 'u':
                            reader.ReadCharacters(unicodebuffer);
                            buffer.Append((char) int.Parse(new string(unicodebuffer), NumberStyles.HexNumber));
                            break;
                        case '\"':
                            buffer.Append(state);
                            state = '\\';
                            break;
                        default:
                            buffer.Append(state);
                            break;
                    }
                    break;
                default:
                    buffer.Append(state);
                    break;
            }
        } while (state != eof && state != '"');

        return buffer.ToString();
    }

    async Task<string> ReadStringAsync(IDataReader reader, AsyncState state) {
        buffer.Length = 0;
        do {
            state.State = await reader.ReadCharacterAsync();
            switch (state.State) {
                case '"':
                    break;
                case '\\':
                    state.State = await reader.ReadCharacterAsync();
                    switch (state.State) {
                        case eof:
                            break;
                        case 't':
                            buffer.Append('\t');
                            break;
                        case 'r':
                            buffer.Append('\r');
                            break;
                        case 'n':
                            buffer.Append('\n');
                            break;
                        case 'u':
                            await reader.ReadCharactersAsync(unicodebuffer, unicodebuffer.Length);
                            buffer.Append((char) int.Parse(new(unicodebuffer), NumberStyles.HexNumber));
                            break;
                        case '\"':
                            buffer.Append(state.State);
                            state.State = '\\';
                            break;
                        default:
                            buffer.Append(state.State);
                            break;
                    }
                    break;
                default:
                    buffer.Append(state.State);
                    break;
            }
        } while (state.State != eof && state.State != '"');

        return buffer.ToString();
    }

    object ReadValue(char firstcharacter, Type type, IDataReader reader, ref char state) {
        buffer.Length = 0;
        buffer.Append(firstcharacter);
            
        do {
            state = reader.ReadCharacter();
            if (char.IsWhiteSpace(state))
                continue;

            if (state == '}' || state == ']' || state == ',' || state== eof)
                break;
                
            buffer.Append(state);
        } while (state != eof);
            
        string value = buffer.ToString();
        object typedvalue = value switch {
                                "null" => null,
                                "true" => true,
                                "false" => false,
                                _ => ToValue(value)
                            };

        if (type == typeof(double) && typedvalue == null)
            return double.NaN;
        if (type != typeof(object))
            typedvalue = Converter.Convert(typedvalue, type);
        return typedvalue;
    }

    object ToValue(string data) {
        if (data.All(char.IsDigit))
            return long.Parse(data, CultureInfo.InvariantCulture);

        return double.Parse(data, CultureInfo.InvariantCulture);
    }
        
    async Task<object> ReadValueAsync(Type type, IDataReader reader, AsyncState state) {
        buffer.Length = 0;
        buffer.Append(state.State);
            
        do {
            state.State = await reader.ReadCharacterAsync();
            if (char.IsWhiteSpace(state.State))
                continue;

            if (state.State == '}' || state.State == ']' || state.State == ',' || state.State== eof)
                break;
                
            buffer.Append(state.State);
        } while (state.State != eof);
            
        string value = buffer.ToString();
        object typedvalue = value switch {
                                "null" => null,
                                "true" => true,
                                "false" => false,
                                _ => ToValue(value)
                            };

        if (type != typeof(object))
            typedvalue = Converter.Convert(typedvalue, type);
        return typedvalue;
    }

}