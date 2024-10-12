using System.Threading.Tasks;

namespace Pooshit.Json.Writer {
    
    /// <summary>
    /// writes json from objects
    /// </summary>
    public interface IJsonWriter {
        
        /// <summary>
        /// writes data as json
        /// </summary>
        /// <param name="data">data to write</param>
        /// <param name="writer">target to write data to</param>
        void Write(object data, IDataWriter writer);
        
        /// <summary>
        /// writes data as json
        /// </summary>
        /// <param name="data">data to write</param>
        /// <param name="writer">target to write data to</param>
        Task WriteAsync(object data, IDataWriter writer);

    }
}