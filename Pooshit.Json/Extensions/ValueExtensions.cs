using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Pooshit.Json.Helpers;

namespace Pooshit.Json.Extensions {
    
    /// <summary>
    /// extensions for value conversions
    /// </summary>
    public static class ValueExtensions {
        
        /**
         * reads a value from a json structure
         */
        public static object ReadStructureValue(this object value, Type targettype, Func<Exception, Type, object, object> errorHandler=null) {
            if (targettype.IsInstanceOfType(value))
                return value;
            
            if (value is IDictionary<string, object> dictionary)
                return dictionary.ReadType(targettype, errorHandler);

            if (!(value is string) && value is IEnumerable childarray)
                return childarray.ReadArray(targettype, errorHandler).Cast<object>().ToArray();

            try {
                return Converter.Convert(value, targettype);
            }
            catch (Exception e) {
                if (errorHandler == null)
                    throw;

                return errorHandler(e, targettype, value);
            }
        }

    }
}