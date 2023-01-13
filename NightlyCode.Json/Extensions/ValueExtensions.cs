using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NightlyCode.Json.Helpers;

namespace NightlyCode.Json.Extensions {
    
    /// <summary>
    /// extensions for value conversions
    /// </summary>
    public static class ValueExtensions {
        
        /**
         * reads a value from a json structure
         */
        public static object ReadStructureValue(this object value, Type targettype) {
            if (targettype.IsInstanceOfType(value))
                return value;
            
            if (value is IDictionary<string, object> dictionary)
                return dictionary.ReadType(targettype);

            if (!(value is string) && value is IEnumerable childarray)
                return childarray.ReadArray(targettype).Cast<object>().ToArray();

            return Converter.Convert(value, targettype);
        }

    }
}