using System.Collections;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;

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
        /// <param name="path">path specifying data to select</param>
        /// <returns>selected data or null if no data matches path</returns>
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
            int index = path.IndexOf('/');
            if (index == -1) {
                return data switch {
                    IDictionary dictionary => dictionary.Contains(path) ? dictionary[path] : null,
                    IEnumerable enumeration => enumeration.Cast<object>().Select(i => Select(i, path)),
                    _ => null
                };
            }

            if (data is IDictionary dic)
                return Select(dic[path.Substring(0, index)], path.Substring(index + 1));

            if (data is IEnumerable enu) {
                return enu.Cast<object>().SelectMany(i => {
                    object result = Select(i, path);
                    if (result is IEnumerable subenum)
                        return subenum.Cast<object>().ToArray();
                    return new[] {result};
                });
            }
            return null;
        }
    }
}