using System;
using System.Threading.Tasks;

namespace NightlyCode.Json.Reader {
    
    /// <summary>
    /// reads json data and deserializes it
    /// </summary>
    public interface IJsonReader {

        /// <summary>
        /// reads json data and converts it to a type
        /// </summary>
        /// <param name="type">type to convert data to</param>
        /// <param name="reader">reader used to retrieve json data</param>
        /// <returns>object data read from json</returns>
        object Read(Type type, IDataReader reader);

        /// <summary>
        /// reads json data and converts it to a type
        /// </summary>
        /// <param name="type">type to convert data to</param>
        /// <param name="reader">reader used to retrieve json data</param>
        /// <returns>object data read from json</returns>
        Task<object> ReadAsync(Type type, IDataReader reader);
    }
}