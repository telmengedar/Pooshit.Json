using System.Threading.Tasks;

namespace NightlyCode.Json.Writer {
    
    /// <summary>
    /// strategy used when writing property names of objects
    /// </summary>
    public interface INamingStrategy {

        /// <summary>
        /// writes a name
        /// </summary>
        /// <param name="name">name to write</param>
        /// <param name="writer">writer to use</param>
        public void WriteName(string name, IDataWriter writer);

        /// <summary>
        /// writes a name
        /// </summary>
        /// <param name="name">name to write</param>
        /// <param name="writer">writer to use</param>
        public Task WriteNameAsync(string name, IDataWriter writer);
    }
}