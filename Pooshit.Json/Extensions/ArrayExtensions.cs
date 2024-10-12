using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Pooshit.Json.Helpers;

namespace Pooshit.Json.Extensions {
    
    /// <summary>
    /// extension methods for arrays
    /// </summary>
    public static class ArrayExtensions {
        
        /**
         * reads a json structure value as array
         */
        public static Array ReadValueAsArray(this object value, Type targettype, Func<Exception, Type, object, object> errorHandler=null) {
            if (value is IDictionary<string, object> dictionary) {
                Array objectarray = Array.CreateInstance(targettype, 1);
                objectarray.SetValue(dictionary.ReadType(targettype, errorHandler), 0);
                return objectarray;
            }

            if (!(value is string) && value is IEnumerable childarray)
                return ReadArray(childarray, targettype, errorHandler);
            
            Array singlearray = Array.CreateInstance(targettype, 1);
            singlearray.SetValue(Converter.Convert(value, targettype), 0);
            return singlearray;
        }

        /// <summary>
        /// reads an array for json deserialization
        /// </summary>
        /// <param name="array">array to deserialize</param>
        /// <param name="elementtype">type of elements to read</param>
        /// <param name="errorHandler">handler used to read value if automatic conversion fails (optional)</param>
        /// <returns>deserialized enumeration</returns>
        public static Array ReadArray(this IEnumerable array, Type elementtype, Func<Exception, Type, object, object> errorHandler=null) {
            object[] objectarray = array as object[] ?? array.Cast<object>().ToArray();
            Array result = Array.CreateInstance(elementtype, objectarray.Length);

            int index = 0;
            if (elementtype.IsArray) {
                Type childelementtype = elementtype.GetElementType();
                foreach (object value in objectarray)
                    result.SetValue(ReadValueAsArray(value, childelementtype, errorHandler), index++);
                return result;
            }

            foreach (object value in objectarray)
                result.SetValue(value.ReadStructureValue(elementtype, errorHandler), index++);
            return result;
        }

        /// <summary>
        /// reads an array for json deserialization
        /// </summary>
        /// <param name="array">array to deserialize</param>
        /// <param name="errorHandler">handler used to read value if automatic conversion fails (optional)</param>
        /// <typeparam name="T">type of elements to read</typeparam>
        /// <returns>deserialized enumeration</returns>
        public static T[] ReadArray<T>(this IEnumerable array, Func<Exception, Type, object, object> errorHandler=null) {
            return (T[])ReadArray(array, typeof(T), errorHandler);
        }
    }
}