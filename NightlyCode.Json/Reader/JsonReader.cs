using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using NightlyCode.Json.Helpers;
using NightlyCode.Json.Tokens;

namespace NightlyCode.Json.Reader {
    
    /// <inheritdoc />
    public class JsonReader : IJsonReader {
        const char eof = '\0';
        readonly StringBuilder buffer=new StringBuilder();
        readonly char[] unicodebuffer=new char[4];

        /// <inheritdoc />
        public object Read(Type type, IDataReader reader) {
            char state = eof;
            return Converter.Convert(Read(type, reader, ref state), type);
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
        
        object ReadObject(Type type, IDataReader reader, ref char state) {
            if (type == typeof(object) || type == typeof(IDictionary)) {
                Dictionary<string, object> dictionary=new Dictionary<string, object>();

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

                    object value = Read(typeof(object), reader, ref state);

                    dictionary[key.ToString()] = value;
                    
                    if (state != '}' && state != ',') {
                        state=reader.ReadCharacter();
                        while (char.IsWhiteSpace(state))
                            state = reader.ReadCharacter();
                    }

                    if (state == ',' || state == '}')
                        break;
                } while (state != eof);

                return dictionary;
            }

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

                PropertyInfo property = type.GetProperty(key.ToString(), BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
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

            state = '\0';
            return result;
        }
        
        Array ReadArray(Type elementtype, IDataReader reader, ref char state) {
            List<object> items=new List<object>();
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
            return value switch {
                "null" => null,
                "true" => true,
                "false" => false,
                _ => Converter.Convert(value, type)
            };
        }
    }
}