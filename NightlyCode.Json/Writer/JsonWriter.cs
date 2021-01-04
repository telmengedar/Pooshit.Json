using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;

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
                    
                    Write(entry.Key.ToString(), writer);
                    writer.WriteCharacter(':');
                    Write(entry.Value, writer);
                }
                writer.WriteCharacter('}');
                return;
            }

            if (data.GetType().IsEnum)
                data = Convert.ChangeType(data, Enum.GetUnderlyingType(data.GetType()));

            if (data is TimeSpan span)
                data = span.ToString("c", CultureInfo.InvariantCulture);
            
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
                string datestring = ((DateTime) data).ToString("o", CultureInfo.InvariantCulture);
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

                /// <inheritdoc />
        public async Task WriteAsync(object data, IDataWriter writer) {
            if (data == null) {
                await writer.WriteStringAsync("null");
                return;
            }

            if (data is Array array) {
                await writer.WriteCharacterAsync('[');
                bool first = true;
                foreach (object item in array) {
                    if (first) first = false;
                    else await writer.WriteCharacterAsync(',');
                    await WriteAsync(item, writer);
                }
                await writer.WriteCharacterAsync(']');
                return;
            }

            if (data is IDictionary dictionary) {
                await writer.WriteCharacterAsync('{');
                bool first = true;
                foreach (DictionaryEntry entry in dictionary) {
                    if (first) first = false;
                    else await writer.WriteCharacterAsync(',');
                    
                    await WriteAsync(entry.Key.ToString(), writer);
                    await writer.WriteCharacterAsync(':');
                    await WriteAsync(entry.Value, writer);
                }
                await writer.WriteCharacterAsync('}');
                return;
            }

            if (data.GetType().IsEnum)
                data = Convert.ChangeType(data, Enum.GetUnderlyingType(data.GetType()));
            
            switch (Type.GetTypeCode(data.GetType())) {
            case TypeCode.Boolean:
                if ((bool) data) await writer.WriteStringAsync("true");
                else await writer.WriteStringAsync("false");
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
                await writer.WriteStringAsync(Convert.ToString(data, CultureInfo.InvariantCulture));
                break;
            case TypeCode.Empty:
            case TypeCode.DBNull:
                await writer.WriteStringAsync("null");
                break;
            case TypeCode.Char:
                await writer.WriteCharacterAsync('"');
                await WriteEscapeValueAsync((char) data, writer);
                await writer.WriteCharacterAsync('"');
                break;
            case TypeCode.DateTime:
                string datestring = Convert.ToString(data, CultureInfo.InvariantCulture);
                await writer.WriteCharacterAsync('"');
                await writer.WriteStringAsync(datestring);
                await writer.WriteCharacterAsync('"');
                break;
            case TypeCode.String:
                await writer.WriteCharacterAsync('"');
                foreach (char character in (string) data)
                    await WriteEscapeValueAsync(character, writer);
                await writer.WriteCharacterAsync('"');
                break;
            case TypeCode.Object:
                if (stringtypes.Contains(data.GetType())) {
                    await writer.WriteCharacterAsync('"');
                    await writer.WriteStringAsync(data.ToString());
                    await writer.WriteCharacterAsync('"');
                }
                else {
                    bool first = true;
                    await writer.WriteCharacterAsync('{');
                    foreach (PropertyInfo property in data.GetType().GetProperties()) {
                        if (!property.CanWrite || !property.CanRead)
                            continue;

                        object value = property.GetValue(data);
                        if (value == null && options.ExcludeNullProperties)
                            continue;
                        
                        if (first) first = false;
                        else await writer.WriteCharacterAsync(',');

                        await writer.WriteCharacterAsync('"');
                        options.NamingStrategy(property.Name, writer);
                        await writer.WriteCharacterAsync('"');
                        
                        await writer.WriteCharacterAsync(':');
                        await WriteAsync(value, writer);
                    }

                    await writer.WriteCharacterAsync('}');
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
            }
        }

        void WriteEscapeValue(char character, IDataWriter writer) {
            switch (character) {
            case '\\':
                writer.WriteString("\\\\");
                break;
            case '\"':
                writer.WriteString("\\\"");
                break;
            case '\t':
                writer.WriteString("\\t");
                break;
            case '\r':
                writer.WriteString("\\r");
                break;
            case '\n':
                writer.WriteString("\\n");
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
        
        Task WriteEscapeValueAsync(char character, IDataWriter writer) {
            switch (character) {
            case '\\':
                return writer.WriteStringAsync("\\\\");
            case '\"':
                return writer.WriteStringAsync("\\\"");
            case '\t':
                return writer.WriteStringAsync("\\t");
            case '\r':
                return writer.WriteStringAsync("\\r");
            case '\n':
                return writer.WriteStringAsync("\\n");
            default:
                if (character < 0x20)
                    return writer.WriteStringAsync($"\\u{((int) character):x4}");
                else
                    return writer.WriteCharacterAsync(character);
            }
        }
    }
}