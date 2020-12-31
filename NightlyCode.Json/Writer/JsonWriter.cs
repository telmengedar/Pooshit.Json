using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace NightlyCode.Json.Writer {
    /// <inheritdoc />
    public class JsonWriter : IJsonWriter {
        readonly HashSet<Type> stringtypes = new HashSet<Type> {
            typeof(Guid)
        };

        readonly JsonOptions options;

        /// <summary>
        /// creates a new <see cref="JsonWriter"/>
        /// </summary>
        public JsonWriter()
        : this(JsonOptions.Default)
        {
        }

        /// <summary>
        /// creates a new <see cref="JsonWriter"/>
        /// </summary>
        /// <param name="options">options used to modify behavior</param>
        public JsonWriter(JsonOptions options) {
            this.options = options;
        }

        /// <inheritdoc />
        public void Write(object data, IDataWriter writer) {
            if (data == null) {
                writer.WriteString("null");
                return;
            }

            if (data is Array array) {
                writer.WriteCharacter('[');
                bool first = true;
                foreach (object item in array) {
                    if (first) first = false;
                    else writer.WriteCharacter(',');
                    Write(item, writer);
                }
                writer.WriteCharacter(']');
                return;
            }

            if (data is IDictionary dictionary) {
                writer.WriteCharacter('{');
                bool first = true;
                foreach (DictionaryEntry entry in dictionary) {
                    if (first) first = false;
                    else writer.WriteCharacter(',');
                    
                    Write(entry.Key.ToString().ToLower(), writer);
                    writer.WriteCharacter(':');
                    Write(entry.Value, writer);
                }
                writer.WriteCharacter('}');
                return;
            }

            if (data.GetType().IsEnum)
                data = Convert.ChangeType(data, Enum.GetUnderlyingType(data.GetType()));
            
            switch (Type.GetTypeCode(data.GetType())) {
            case TypeCode.Boolean:
                if ((bool) data) writer.WriteString("true");
                else writer.WriteString("false");
                break;
            case TypeCode.SByte:
            case TypeCode.Byte:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.UInt64:
            case TypeCode.Decimal:
            case TypeCode.Single:
            case TypeCode.Double:
                writer.WriteString(Convert.ToString(data, CultureInfo.InvariantCulture));
                break;
            case TypeCode.Empty:
            case TypeCode.DBNull:
                writer.WriteString("null");
                break;
            case TypeCode.Char:
                writer.WriteCharacter('"');
                WriteEscapeValue((char) data, writer);
                writer.WriteCharacter('"');
                break;
            case TypeCode.DateTime:
                string datestring = Convert.ToString(data, CultureInfo.InvariantCulture);
                writer.WriteCharacter('"');
                writer.WriteString(datestring);
                writer.WriteCharacter('"');
                break;
            case TypeCode.String:
                writer.WriteCharacter('"');
                foreach (char character in (string) data)
                    WriteEscapeValue(character, writer);
                writer.WriteCharacter('"');
                break;
            case TypeCode.Object:
                if (stringtypes.Contains(data.GetType())) {
                    writer.WriteCharacter('"');
                    writer.WriteString(data.ToString());
                    writer.WriteCharacter('"');
                }
                else {
                    bool first = true;
                    writer.WriteCharacter('{');
                    foreach (PropertyInfo property in data.GetType().GetProperties()) {
                        if (!property.CanWrite || !property.CanRead)
                            continue;

                        object value = property.GetValue(data);
                        if (value == null && options.ExcludeNullProperties)
                            continue;
                        
                        if (first) first = false;
                        else writer.WriteCharacter(',');

                        writer.WriteCharacter('"');
                        options.NamingStrategy(property.Name, writer);
                        writer.WriteCharacter('"');
                        
                        writer.WriteCharacter(':');
                        Write(value, writer);
                    }

                    writer.WriteCharacter('}');
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
            }
        }

        void WriteEscapeValue(char character, IDataWriter writer) {
            switch (character) {
            case '\\':
                writer.WriteCharacter('\\');
                break;
            case '\"':
                writer.WriteString("\\\"");
                break;
            case '\t':
                writer.WriteCharacter('\t');
                break;
            case '\r':
                writer.WriteCharacter('\r');
                break;
            case '\n':
                writer.WriteCharacter('\n');
                break;
            default:
                if (character < 0x20) {
                    writer.WriteString("\\u");
                    writer.WriteString(((int) character).ToString("x4"));
                }
                else
                    writer.WriteCharacter(character);
                break;
            }
        }
    }
}