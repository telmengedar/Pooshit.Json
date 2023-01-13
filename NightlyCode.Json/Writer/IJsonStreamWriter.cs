using System.Threading.Tasks;

namespace NightlyCode.Json.Writer {
    
    /// <summary>
    /// writes json data to a stream
    /// </summary>
    public interface IJsonStreamWriter {
        
        /// <summary>
        /// writes array start to the stream
        /// </summary>
        void BeginArray();
        
        /// <summary>
        /// writes array end to the stream
        /// </summary>
        void EndArray();
        
        /// <summary>
        /// writes object start to the stream
        /// </summary>
        void BeginObject();
        
        /// <summary>
        /// writes array end to the stream
        /// </summary>
        void EndObject();

        /// <summary>
        /// writes key name of a property
        /// </summary>
        /// <param name="key">key to write</param>
        void WriteKey(string key);
        
        /// <summary>
        /// writes a property to the stream
        /// </summary>
        void WriteProperty(string key, object value);
        
        /// <summary>
        /// writes a value to the stream
        /// </summary>
        void WriteValue(object value);
        
        /// <summary>
        /// writes array start to the stream
        /// </summary>
        Task BeginArrayAsync();
        
        /// <summary>
        /// writes array end to the stream
        /// </summary>
        Task EndArrayAsync();
        
        /// <summary>
        /// writes object start to the stream
        /// </summary>
        Task BeginObjectAsync();
        
        /// <summary>
        /// writes array end to the stream
        /// </summary>
        Task EndObjectAsync();
        
        /// <summary>
        /// writes key name of a property
        /// </summary>
        /// <param name="key">key to write</param>
        Task WriteKeyAsync(string key);

        /// <summary>
        /// writes a property to the stream
        /// </summary>
        Task WritePropertyAsync(string key, object value);
        
        /// <summary>
        /// writes a value to the stream
        /// </summary>
        Task WriteValueAsync(object value);
    }
}