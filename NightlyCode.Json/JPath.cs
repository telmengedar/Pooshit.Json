using System.Collections;
using System.Linq;

namespace NightlyCode.Json {
    
    /// <summary>
    /// class providing jpath functionality
    /// </summary>
    /// <remarks>
    /// this just behaves like jpath as needed and is not a proper jpath implementation
    /// </remarks>
    public static class JPath {

        /// <summary>
        /// retrieve values from a json structure
        /// </summary>
        /// <param name="data">json structure to select data from</param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static T Select<T>(object data, string path) {
            object value = Select(data, path);
            if (value is T typedvalue)
                return typedvalue;
            return default;
        }

        /// <summary>
        /// retrieve values from a json structure
        /// </summary>
        /// <param name="data">json structure to select data from</param>
        /// <param name="path">path specifying element to select</param>
        /// <returns>result of path selection</returns>
        public static object Select(object data, string path) {
            return data switch {
                IDictionary dictionary => dictionary.Contains(path) ? dictionary[path] : null,
                IEnumerable enumeration => enumeration.Cast<object>().Select(i => Select(i, path)),
                _ => null
            };
        }
    }
}