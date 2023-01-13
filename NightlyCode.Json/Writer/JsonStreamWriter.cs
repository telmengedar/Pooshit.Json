using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace NightlyCode.Json.Writer {
    
    /// <inheritdoc />
    public class JsonStreamWriter : IJsonStreamWriter, IDisposable
#if NETSTANDARD2_1
    , IAsyncDisposable
#endif
    {
        readonly HashSet<Type> stringtypes = new HashSet<Type> {
            typeof(Guid),
            typeof(IPAddress)
        };

        readonly StreamWriter writer;
        readonly JsonOptions options=JsonOptions.Default;
        
        int state;
        
        /// <summary>
        /// creates a new <see cref="JsonStreamWriter"/>
        /// </summary>
        /// <param name="targetStream">strema to write data to</param>
        public JsonStreamWriter(Stream targetStream) {
            writer = new StreamWriter(targetStream);
        }

        /// <summary>
        /// creates a new <see cref="JsonStreamWriter"/>
        /// </summary>
        /// <param name="targetStream">strema to write data to</param>
        /// <param name="options">options for writer</param>
        public JsonStreamWriter(Stream targetStream, JsonOptions options)
            : this(targetStream) {
            this.options = options;
        }

        /// <inheritdoc />
        public void BeginArray() {
            WriteState();
            writer.Write("[");
            state = 0;
        }

        /// <inheritdoc />
        public void EndArray() {
            writer.Write("]");
            state = 1;
        }

        /// <inheritdoc />
        public void BeginObject() {
            WriteState();
            writer.Write("{");
            state = 0;
        }

        /// <inheritdoc />
        public void EndObject() {
            writer.Write("}");
            state = 1;
        }

        void WriteState() {
            switch (state) {
                case 1:
                    writer.Write(',');
                    break;
                case 2:
                    writer.Write(':');
                    break;
            }

            state = 0;
        }

        async Task WriteStateAsync() {
            switch(state) {
                case 1:
                    await writer.WriteAsync(',');
                    break;
                case 2:
                    await writer.WriteAsync(':');
                    break;
            }

            state = 0;
        }

        /// <inheritdoc />
        public void WriteKey(string key) {
            if (state > 1)
                throw new InvalidOperationException("A key cannot follow a key in a json stream");
            WriteState();
            writer.Write($"\"{key}\"");
            state = 2;
        }

        /// <inheritdoc />
        public void WriteProperty(string key, object value) {
            WriteKey(key);
            WriteValue(value);
        }

        void WriteEscapeValue(char character) {
            switch (character) {
                case '\\':
                    writer.Write("\\\\");
                    break;
                case '\"':
                    writer.Write("\\\"");
                    break;
                case '\t':
                    writer.Write("\\t");
                    break;
                case '\r':
                    writer.Write("\\r");
                    break;
                case '\n':
                    writer.Write("\\n");
                    break;
                default:
                    if (character < 0x20) {
                        writer.Write("\\u");
                        writer.Write(((int) character).ToString("x4"));
                    }
                    else
                        writer.Write(character);
                    break;
            }
        }

        /// <inheritdoc />
        public void WriteValue(object data) {
            if (data == null) {
                writer.Write("null");
                return;
            }

            if (data is IDictionary dictionary) {
                BeginObject();
                foreach (DictionaryEntry entry in dictionary)
                    WriteProperty(entry.Key.ToString(), entry.Value);
                EndObject();
                return;
            }

            if (!(data is string) && data is IEnumerable array) {
                BeginArray();
                foreach (object item in array)
                    WriteValue(item);
                EndArray();
                return;
            }
            
            if (data.GetType().IsEnum) {
                if (options.WriteEnumsAsStrings)
                    data = data.ToString();
                else data = Convert.ChangeType(data, Enum.GetUnderlyingType(data.GetType()));
            }

            if (data is TimeSpan span)
                data = span.ToString("c", CultureInfo.InvariantCulture);
            
            WriteState();
            switch (Type.GetTypeCode(data.GetType())) {
                case TypeCode.Boolean:
                    if ((bool) data) writer.Write("true");
                    else writer.Write("false");
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
                    writer.Write(Convert.ToString(data, CultureInfo.InvariantCulture));
                    break;
                case TypeCode.Single:
                    if (float.IsNaN((float)data) || float.IsInfinity((float)data))
                        writer.Write("null");
                    else writer.Write(Convert.ToString(data, CultureInfo.InvariantCulture));
                    break;
                case TypeCode.Double:
                    if (double.IsNaN((double)data) || double.IsInfinity((double)data))
                        writer.Write("null");
                    else writer.Write(Convert.ToString(data, CultureInfo.InvariantCulture));
                    break;
                case TypeCode.Empty:
                case TypeCode.DBNull:
                    writer.Write("null");
                    break;
                case TypeCode.Char:
                    writer.Write('"');
                    WriteEscapeValue((char) data);
                    writer.Write('"');
                    break;
                case TypeCode.DateTime:
                    string datestring = ((DateTime) data).ToString("o", CultureInfo.InvariantCulture);
                    writer.Write('"');
                    writer.Write(datestring);
                    writer.Write('"');
                    break;
                case TypeCode.String:
                    writer.Write('"');
                    foreach (char character in (string) data)
                        WriteEscapeValue(character);
                    writer.Write('"');
                    break;
                case TypeCode.Object:
                    if (stringtypes.Contains(data.GetType())) {
                        writer.Write('"');
                        writer.Write(data.ToString());
                        writer.Write('"');
                    }
                    else {
                        BeginObject();
                        foreach (PropertyInfo property in data.GetType().GetProperties()) {
                            if (!property.CanWrite || !property.CanRead || property.GetIndexParameters().Length > 0 || Attribute.IsDefined(property, typeof(IgnoreDataMemberAttribute)))
                                continue;

                            object value = property.GetValue(data);
                            if (value == null && options.ExcludeNullProperties)
                                continue;

                            WriteProperty(options.NamingStrategy.GenerateName(property.Name), value);
                        }
                        EndObject();
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            state = 1;
        }

        /// <inheritdoc />
        public async Task BeginArrayAsync() {
            await WriteStateAsync();
            await writer.WriteAsync("[");
            state = 0;
        }

        /// <inheritdoc />
        public async Task EndArrayAsync() {
            await writer.WriteAsync("]");
            state = 1;
        }

        /// <inheritdoc />
        public async Task BeginObjectAsync() {
            await WriteStateAsync();
            await writer.WriteAsync("{");
            state = 0;
        }

        /// <inheritdoc />
        public async Task EndObjectAsync() {
            await writer.WriteAsync("}");
            state = 1;
        }

        /// <inheritdoc />
        public async Task WriteKeyAsync(string key) {
            if (state > 1)
                throw new InvalidOperationException("A key cannot follow a key in a json stream");
            await WriteStateAsync();
            await writer.WriteAsync($"\"{key}\"");
            state = 2;
        }

        /// <inheritdoc />
        public async Task WritePropertyAsync(string key, object value) {
            await WriteKeyAsync(key);
            await WriteValueAsync(value);
        }

        Task WriteEscapeValueAsync(char character) {
            switch (character) {
                case '\\':
                    return writer.WriteAsync("\\\\");
                case '\"':
                    return writer.WriteAsync("\\\"");
                case '\t':
                    return writer.WriteAsync("\\t");
                case '\r':
                    return writer.WriteAsync("\\r");
                case '\n':
                    return writer.WriteAsync("\\n");
                default:
                    if (character < 0x20)
                        return writer.WriteAsync($"\\u{(int)character:x4}");
                    else
                        return writer.WriteAsync(character);
            }
        }

        /// <inheritdoc />
        public async Task WriteValueAsync(object data) {
            if (data == null) {
                await writer.WriteAsync("null");
                return;
            }

            if (data is IDictionary dictionary) {
                await BeginObjectAsync();
                foreach (DictionaryEntry entry in dictionary)
                    await WritePropertyAsync(entry.Key.ToString(), entry.Value);
                await EndObjectAsync();
                return;
            }

            if (!(data is string) && data is IEnumerable array) {
                await BeginArrayAsync();
                foreach (object item in array)
                    await WriteValueAsync(item);
                await EndArrayAsync();
                return;
            }
            
            if (data.GetType().IsEnum) {
                if (options.WriteEnumsAsStrings)
                    data = data.ToString();
                else data = Convert.ChangeType(data, Enum.GetUnderlyingType(data.GetType()));
            }

            if (data is TimeSpan span)
                data = span.ToString("c", CultureInfo.InvariantCulture);
            
            await WriteStateAsync();
            switch (Type.GetTypeCode(data.GetType())) {
                case TypeCode.Boolean:
                    if ((bool) data) await writer.WriteAsync("true");
                    else await writer.WriteAsync("false");
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
                    await writer.WriteAsync(Convert.ToString(data, CultureInfo.InvariantCulture));
                    break;
                case TypeCode.Single:
                    if (float.IsNaN((float)data) || float.IsInfinity((float)data))
                        await writer.WriteAsync("null");
                    else await writer.WriteAsync(Convert.ToString(data, CultureInfo.InvariantCulture));
                    break;
                case TypeCode.Double:
                    if (double.IsNaN((double)data) || double.IsInfinity((double)data))
                        await writer.WriteAsync("null");
                    else await writer.WriteAsync(Convert.ToString(data, CultureInfo.InvariantCulture));
                    break;
                case TypeCode.Empty:
                case TypeCode.DBNull:
                    await writer.WriteAsync("null");
                    break;
                case TypeCode.Char:
                    await writer.WriteAsync('"');
                    await WriteEscapeValueAsync((char) data);
                    await writer.WriteAsync('"');
                    break;
                case TypeCode.DateTime:
                    string datestring = ((DateTime) data).ToString("o", CultureInfo.InvariantCulture);
                    await writer.WriteAsync('"');
                    await writer.WriteAsync(datestring);
                    await writer.WriteAsync('"');
                    break;
                case TypeCode.String:
                    await writer.WriteAsync('"');
                    foreach (char character in (string) data)
                        await WriteEscapeValueAsync(character);
                    await writer.WriteAsync('"');
                    break;
                case TypeCode.Object:
                    if (stringtypes.Contains(data.GetType())) {
                        await writer.WriteAsync('"');
                        await writer.WriteAsync(data.ToString());
                        await writer.WriteAsync('"');
                    }
                    else {
                        await BeginObjectAsync();
                        foreach (PropertyInfo property in data.GetType().GetProperties()) {
                            if (!property.CanWrite || !property.CanRead || property.GetIndexParameters().Length > 0 || Attribute.IsDefined(property, typeof(IgnoreDataMemberAttribute)))
                                continue;

                            object value = property.GetValue(data);
                            if (value == null && options.ExcludeNullProperties)
                                continue;

                            await WritePropertyAsync(options.NamingStrategy.GenerateName(property.Name), value);
                        }
                        await EndObjectAsync();
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            state = 1;
        }

        /// <inheritdoc />
        public void Dispose() {
            //targetStream?.Dispose();
            writer.Dispose();
        }

#if NETSTANDARD2_1
        public ValueTask DisposeAsync() {
            return writer.DisposeAsync();
        }
#endif
    }
}