using System.Threading.Tasks;

namespace NightlyCode.Json.Writer.Naming {
    
    /// <summary>
    /// default naming strategy used by serializer which does not modify anything and writes the name as is
    /// </summary>
    public class DefaultNamingStrategy : INamingStrategy {
        
        /// <inheritdoc />
        public void WriteName(string name, IDataWriter writer) {
            writer.WriteString(name);
        }

        /// <inheritdoc />
        public Task WriteNameAsync(string name, IDataWriter writer) {
            return writer.WriteStringAsync(name);
        }
    }
}