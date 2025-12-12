using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Pooshit.Json.Extensions;
using Pooshit.Json.Models;
using Pooshit.Json.Writer.Naming;
using Pooshit.Reflection;
using Model = Pooshit.Json.Models.Model;

namespace Pooshit.Json.Writer;

/// <inheritdoc />
public class JsonWriter : IJsonWriter {
    readonly HashSet<Type> stringtypes = [typeof(Guid)];

    readonly JsonOptions options;
    int indentation;
        
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
        this.options.NamingStrategy ??= new DefaultNamingStrategy();
    }

    /// <inheritdoc />
    public void Write(object data, IDataWriter writer) {
        if (data == null) {
            writer.WriteString("null");
            return;
        }

        if (data is IDictionary dictionary) {
            writer.WriteCharacter('{');
            bool first = true;
                
            if (options.FormatOutput) {
                ++indentation;
                writer.WriteCharacter('\n');
                        
            }

            foreach (DictionaryEntry entry in dictionary) {
                if (entry.Value == null && options.ExcludeNullProperties)
                    continue;

                if (first) first = false;
                else {
                    writer.WriteCharacter(',');
                    if(options.FormatOutput)
                        writer.WriteCharacter('\n');
                }

                if(options.FormatOutput)
                    writer.WriteString(new('\t', indentation));

                writer.WriteCharacter('"');
                options.NamingStrategy.WriteName(entry.Key.ToString(), writer);
                writer.WriteCharacter('"');
                
                writer.WriteCharacter(':');
                Write(entry.Value, writer);
            }
            writer.WriteCharacter('}');
            return;
        }

        if (!(data is string) && data is IEnumerable array) {
            if (data is byte[] binary && options.ByteArrayBehavior!=ByteArrayBehavior.Keep) {
                switch (options.ByteArrayBehavior) {
                    case ByteArrayBehavior.Strip:
                        Write(null, writer);
                    break;
                    case ByteArrayBehavior.Base64:
                        Write(Convert.ToBase64String(binary), writer);
                    break;
                }
                return;
            }
            
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
            
        if (data.GetType().IsEnum) {
            if (options.WriteEnumsAsStrings)
                data = data.ToString();
            else data = Convert.ChangeType(data, Enum.GetUnderlyingType(data.GetType()));
        }

        if (data is TimeSpan span)
            data = span.ToString("c", CultureInfo.InvariantCulture);

        switch (Type.GetTypeCode(data.GetType())) {
            case TypeCode.Boolean:
                if ((bool)data) writer.WriteString("true");
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
                writer.WriteString(Convert.ToString(data, CultureInfo.InvariantCulture));
            break;
            case TypeCode.Single:
                if (float.IsNaN((float)data) || float.IsInfinity((float)data))
                    writer.WriteString("null");
                else writer.WriteString(Convert.ToString(data, CultureInfo.InvariantCulture));
            break;
            case TypeCode.Double:
                if (double.IsNaN((double)data) || double.IsInfinity((double)data))
                    writer.WriteString("null");
                else writer.WriteString(Convert.ToString(data, CultureInfo.InvariantCulture));
            break;
            case TypeCode.Empty:
            case TypeCode.DBNull:
                writer.WriteString("null");
            break;
            case TypeCode.Char:
                writer.WriteCharacter('"');
                WriteEscapeValue((char)data, writer);
                writer.WriteCharacter('"');
            break;
            case TypeCode.DateTime:
                string datestring = ((DateTime)data).ToString("o", CultureInfo.InvariantCulture);
                writer.WriteCharacter('"');
                writer.WriteString(datestring);
                writer.WriteCharacter('"');
            break;
            case TypeCode.String:
                writer.WriteCharacter('"');
                foreach (char character in (string)data)
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
                    if (options.FormatOutput) {
                        ++indentation;
                        writer.WriteCharacter('\n');

                    }

                    IModel model = Model.GetModel(data.GetType());
                    foreach (IPropertyInfo property in model.Properties) {
                        if (!property.HasSetter || !property.HasGetter || property.Attributes.Any(a => a is IgnoreDataMemberAttribute))
                            continue;

                        object value = property.GetValue(data);
                        if (value == null && options.ExcludeNullProperties)
                            continue;

                        if (first) first = false;
                        else {
                            writer.WriteCharacter(',');
                            if (options.FormatOutput)
                                writer.WriteCharacter('\n');
                        }

                        if (options.FormatOutput)
                            writer.WriteString(new('\t', indentation));

                        writer.WriteCharacter('"');
                        options.NamingStrategy.WriteName(property.Name, writer);
                        writer.WriteCharacter('"');

                        writer.WriteCharacter(':');
                        Write(value, writer);
                    }

                    if (options.FormatOutput) {
                        writer.WriteCharacter('\n');
                        --indentation;
                        writer.WriteString(new('\t', indentation));
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

        if (data is IDictionary dictionary) {
            await writer.WriteCharacterAsync('{');
            bool first = true;
            foreach (DictionaryEntry entry in dictionary) {
                if (entry.Value == null && options.ExcludeNullProperties)
                    continue;

                if (first) first = false;
                else await writer.WriteCharacterAsync(',');
                    
                await writer.WriteCharacterAsync('"');
                await options.NamingStrategy.WriteNameAsync(entry.Key.ToString(), writer);
                await writer.WriteCharacterAsync('"');

                await writer.WriteCharacterAsync(':');
                await WriteAsync(entry.Value, writer);
            }
            await writer.WriteCharacterAsync('}');
            return;
        }

#if NETSTANDARD2_1
        if(data.IsAsyncEnumerable()) {
            await writer.WriteCharacterAsync('[');
            bool first = true;
            Tuple<IAsyncDisposable, MethodInfo, PropertyInfo> enumerator = data.GetAsyncEnumerator();
            if (enumerator != null) {
                await using (enumerator.Item1) {
                    while (await (ValueTask<bool>)enumerator.Item2.Invoke(enumerator.Item1, null)) {
                        if (first) first = false;
                        else await writer.WriteCharacterAsync(',');
                        await WriteAsync(enumerator.Item3.GetValue(enumerator.Item1), writer);
                    }
                }
            }

            await writer.WriteCharacterAsync(']');
            return;
        }
#endif

        if (data is not string && data is IEnumerable array) {
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
        
        if (data.GetType().IsEnum) {
            if (options.WriteEnumsAsStrings)
                data = data.ToString();
            else data = Convert.ChangeType(data, Enum.GetUnderlyingType(data.GetType()));
        }

        if (data is TimeSpan span)
            data = span.ToString("c", CultureInfo.InvariantCulture);

        switch (Type.GetTypeCode(data.GetType())) {
            case TypeCode.Boolean:
                if ((bool)data) await writer.WriteStringAsync("true");
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
                await WriteEscapeValueAsync((char)data, writer);
                await writer.WriteCharacterAsync('"');
            break;
            case TypeCode.DateTime:
                string datestring = ((DateTime)data).ToString("o", CultureInfo.InvariantCulture);
                await writer.WriteCharacterAsync('"');
                await writer.WriteStringAsync(datestring);
                await writer.WriteCharacterAsync('"');
            break;
            case TypeCode.String:
                await writer.WriteCharacterAsync('"');
                foreach (char character in (string)data)
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

                    IModel model = Model.GetModel(data.GetType());
                    foreach (IPropertyInfo property in model.Properties) {
                        if (!property.HasSetter || !property.HasGetter || property.Attributes.Any(a => a is IgnoreDataMemberAttribute))
                            continue;

                        object value = property.GetValue(data);
                        if (value == null && options.ExcludeNullProperties)
                            continue;

                        if (first) first = false;
                        else await writer.WriteCharacterAsync(',');

                        await writer.WriteCharacterAsync('"');
                        await options.NamingStrategy.WriteNameAsync(property.Name, writer);
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