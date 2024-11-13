using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Pooshit.Json.Extensions;
using Pooshit.Json.Helpers;
using Pooshit.Json.Reader;
using Pooshit.Json.Writer;

namespace Pooshit.Json;

/// <summary>
/// static interface for json operations
/// </summary>
public static class Json {

    /// <summary>
    /// registers a custom converter to be used for a specific conversion
    /// </summary>
    /// <param name="targettype">type to convert value to</param>
    /// <param name="converter">delegate used to convert value</param>
    /// <param name="sourcetype">type of value from which to convert</param>
    public static void SetCustomConverter(Type sourcetype, Type targettype, Func<object, object> converter) {
        Converter.RegisterConverter(sourcetype, targettype, converter);
    }

    /// <summary>
    /// writes data to a json string
    /// </summary>
    /// <param name="data">data to write</param>
    /// <returns>json representation</returns>
    public static string WriteString(object data) {
        return WriteString(data, JsonOptions.Default);
    }

    /// <summary>
    /// writes data to a json string
    /// </summary>
    /// <param name="data">data to write</param>
    /// <returns>json representation</returns>
    public static Task<string> WriteStringAsync(object data) {
        return WriteStringAsync(data, JsonOptions.Default);
    }

    /// <summary>
    /// writes data to a json string
    /// </summary>
    /// <param name="data">data to write</param>
    /// <param name="options">options used to modify json result</param>
    /// <returns>json representation</returns>
    public static string WriteString(object data, JsonOptions options) {
        StringBuilder builder = new();
        using (StringWriter stringwriter = new(builder)) {
            new JsonWriter(options).Write(data, new DataWriter(stringwriter));
        }

        return builder.ToString();
    }

    /// <summary>
    /// writes data to a json string
    /// </summary>
    /// <param name="data">data to write</param>
    /// <param name="options">options used to modify json result</param>
    /// <returns>json representation</returns>
    public static async Task<string> WriteStringAsync(object data, JsonOptions options) {
        StringBuilder builder = new();
        using (StringWriter stringwriter = new(builder))
            await new JsonWriter(options).WriteAsync(data, new DataWriter(stringwriter));

        return builder.ToString();
    }

    /// <summary>
    /// writes data as json to a stream
    /// </summary>
    /// <param name="data">data to write</param>
    /// <param name="target">stream to write json to</param>
    public static void Write(object data, Stream target) {
        Write(data, target, JsonOptions.Default);
    }

    /// <summary>
    /// writes data as json to a stream
    /// </summary>
    /// <param name="data">data to write</param>
    /// <param name="target">stream to write json to</param>
    public static Task WriteAsync(object data, Stream target) {
        return WriteAsync(data, target, JsonOptions.Default);
    }

    /// <summary>
    /// writes data as json to a stream
    /// </summary>
    /// <param name="data">data to write</param>
    /// <param name="target">stream to write json to</param>
    /// <param name="options">options used to modify json result</param>
    public static void Write(object data, Stream target, JsonOptions options) {
        using StreamWriter streamwriter = new(target, options.Encoding ?? new UTF8Encoding(false), 1024, true);
        new JsonWriter(options).Write(data, new DataWriter(streamwriter));
    }

    /// <summary>
    /// writes data as json to a stream
    /// </summary>
    /// <param name="data">data to write</param>
    /// <param name="target">stream to write json to</param>
    /// <param name="options">options used to modify json result</param>
    public static void Write(object data, TextWriter target, JsonOptions options) {
        new JsonWriter(options).Write(data, new DataWriter(target));
    }

    /// <summary>
    /// writes data as json to a stream
    /// </summary>
    /// <param name="data">data to write</param>
    /// <param name="target">stream to write json to</param>
    /// <param name="options">options used to modify json result</param>
    public static async Task WriteAsync(object data, Stream target, JsonOptions options) {
#if NETSTANDARD2_1
            await
#endif
        using StreamWriter streamwriter = new(target, options.Encoding ?? new UTF8Encoding(false), 1024, true);
        await WriteAsync(data, streamwriter, options);
#if !NETSTANDARD2_1
        await streamwriter.FlushAsync();
#endif
    }

    /// <summary>
    /// writes data as json to a stream
    /// </summary>
    /// <param name="data">data to write</param>
    /// <param name="target">stream to write json to</param>
    /// <param name="options">options used to modify json result</param>
    public static Task WriteAsync(object data, TextWriter target, JsonOptions options) {
        return new JsonWriter(options).WriteAsync(data, new DataWriter(target));
    }

    /// <summary>
    /// reads an object from a json string
    /// </summary>
    /// <param name="type">type of data to read</param>
    /// <param name="data">string to read data from</param>
    /// <returns>converted data</returns>
    public static object Read(Type type, string data) {
        using TextReader textreader = new StringReader(data);
        return new JsonReader().Read(type, new DataReader(textreader));
    }

    /// <summary>
    /// reads an object from a json string
    /// </summary>
    /// <param name="type">type of data to read</param>
    /// <param name="data">string to read data from</param>
    /// <returns>converted data</returns>
    public static async Task<object> ReadAsync(Type type, string data) {
        using TextReader textreader = new StringReader(data);
        return await new JsonReader().ReadAsync(type, new DataReader(textreader));
    }

    /// <summary>
    /// reads an object from a json string
    /// </summary>
    /// <param name="data">string to read data from</param>
    /// <typeparam name="T">type of data to read</typeparam>
    /// <returns>converted data</returns>
    public static T Read<T>(string data) {
        return (T)Read(typeof(T), data);
    }

    /// <summary>
    /// reads an object from a json string
    /// </summary>
    /// <param name="data">string to read data from</param>
    /// <typeparam name="T">type of data to read</typeparam>
    /// <returns>converted data</returns>
    public static async Task<T> ReadAsync<T>(string data) {
        object value = await ReadAsync(typeof(T), data);
        return (T)value;
    }

    /// <summary>
    /// reads an object from a json string
    /// </summary>
    /// <param name="data">string to read data from</param>
    /// <returns>converted data</returns>
    public static object Read(string data) {
        return Read(typeof(object), data);
    }

    /// <summary>
    /// reads an object from a json string
    /// </summary>
    /// <param name="data">string to read data from</param>
    /// <returns>converted data</returns>
    public static Task<object> ReadAsync(string data) {
        return ReadAsync(typeof(object), data);
    }

    /// <summary>
    /// reads an object from a json stream
    /// </summary>
    /// <param name="type">type of data to read</param>
    /// <param name="data">string to read data from</param>
    /// <returns>converted data</returns>
    public static object Read(Type type, Stream data) {
        return Read(type, data, JsonOptions.Default);
    }

    /// <summary>
    /// reads an object from a json stream
    /// </summary>
    /// <param name="type">type of data to read</param>
    /// <param name="data">string to read data from</param>
    /// <param name="options">options to use when reading</param>
    /// <returns>converted data</returns>
    public static object Read(Type type, Stream data, JsonOptions options) {
        using TextReader textreader = new StreamReader(data, options.Encoding ?? Encoding.UTF8, true, 1024, true);
        return new JsonReader().Read(type, new DataReader(textreader));
    }

    /// <summary>
    /// reads an object from a json structure
    /// </summary>
    /// <param name="type">type of data to read</param>
    /// <param name="structure">json data. should be a string, a stream, an idictionary or an array</param>
    /// <param name="errorHandler">handler used to read value if automatic conversion fails (optional)</param>
    /// <returns>read data</returns>
    public static object Read(Type type, object structure, Func<Exception, Type, object, object> errorHandler=null) {
        if (structure == null)
            return null;
        if (type.IsInstanceOfType(structure))
            return structure;

        if (structure is string stringdata)
            return Read(type, stringdata);
        if (structure is Stream streamdata)
            return Read(type, streamdata);
        if (structure is IDictionary<string, object> dictionary)
            return dictionary.ReadType(type, errorHandler);
        if (structure is IEnumerable array)
            return array.ReadArray(type.GetElementType(), errorHandler);
        return Converter.Convert(structure, type);
    }

    /// <summary>
    /// reads an object from a json structure
    /// </summary>
    /// <param name="structure">json data. should be a string, a stream, an idictionary or an array</param>
    /// <param name="errorHandler">handler used to read value if automatic conversion fails (optional)</param>
    /// <typeparam name="T">type of data to read</typeparam>
    /// <returns>data read from structure</returns>
    public static T Read<T>(object structure, Func<Exception, Type, object, object> errorHandler=null) {
        return (T)Read(typeof(T), structure, errorHandler);
    }

    /// <summary>
    /// reads an object from a json stream
    /// </summary>
    /// <param name="type">type of data to read</param>
    /// <param name="data">string to read data from</param>
    /// <returns>converted data</returns>
    public static Task<object> ReadAsync(Type type, Stream data) {
        return ReadAsync(type, data, JsonOptions.Default);
    }

    /// <summary>
    /// reads an object from a json stream
    /// </summary>
    /// <param name="type">type of data to read</param>
    /// <param name="data">string to read data from</param>
    /// <param name="options">options to use when reading json</param>
    /// <returns>converted data</returns>
    public static async Task<object> ReadAsync(Type type, Stream data, JsonOptions options) {
        using StreamReader textreader = new StreamReader(data, options.Encoding ?? Encoding.UTF8, true, 1024, true);
        return await new JsonReader().ReadAsync(type, new DataReader(textreader));
    }

    /// <summary>
    /// reads an object from a json stream
    /// </summary>
    /// <param name="data">string to read data from</param>
    /// <typeparam name="T">type of data to read</typeparam>
    /// <returns>converted data</returns>
    public static T Read<T>(Stream data) {
        return (T)Read(typeof(T), data);
    }

    /// <summary>
    /// reads an object from a json stream
    /// </summary>
    /// <param name="data">string to read data from</param>
    /// <param name="options">options to use when reading json</param>
    /// <typeparam name="T">type of data to read</typeparam>
    /// <returns>converted data</returns>
    public static T Read<T>(Stream data, JsonOptions options) {
        return (T)Read(typeof(T), data, options);
    }

    /// <summary>
    /// reads an object from a json stream
    /// </summary>
    /// <param name="data">string to read data from</param>
    /// <typeparam name="T">type of data to read</typeparam>
    /// <returns>converted data</returns>
    public static async Task<T> ReadAsync<T>(Stream data) {
        object value = await ReadAsync(typeof(T), data);
        return (T)value;
    }

    /// <summary>
    /// reads an object from a json stream
    /// </summary>
    /// <param name="data">string to read data from</param>
    /// <param name="options">options to use when reading json</param>
    /// <typeparam name="T">type of data to read</typeparam>
    /// <returns>converted data</returns>
    public static async Task<T> ReadAsync<T>(Stream data, JsonOptions options) {
        object value = await ReadAsync(typeof(T), data, options);
        return (T)value;
    }

    /// <summary>
    /// reads an object from a json stream
    /// </summary>
    /// <param name="data">string to read data from</param>
    /// <returns>converted data</returns>
    public static object Read(Stream data) {
        return Read(typeof(object), data);
    }

    /// <summary>
    /// reads an object from a json stream
    /// </summary>
    /// <param name="data">string to read data from</param>
    /// <param name="options">options to use when reading json</param>
    /// <returns>converted data</returns>
    public static object Read(Stream data, JsonOptions options) {
        return Read(typeof(object), data, options);
    }

    /// <summary>
    /// reads an object from a json stream
    /// </summary>
    /// <param name="data">string to read data from</param>
    /// <returns>converted data</returns>
    public static Task<object> ReadAsync(Stream data) {
        return ReadAsync(typeof(object), data);
    }

    /// <summary>
    /// reads an object from a json stream
    /// </summary>
    /// <param name="data">string to read data from</param>
    /// <param name="options">options to use when reading json</param>
    /// <returns>converted data</returns>
    public static Task<object> ReadAsync(Stream data, JsonOptions options) {
        return ReadAsync(typeof(object), data, options);
    }

}