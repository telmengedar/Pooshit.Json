using System.Threading.Tasks;

namespace NightlyCode.Json.Writer.Naming {
    
    /// <summary>
    /// writes names in all lowercase
    /// </summary>
    public class LowerCaseNamingStrategy : INamingStrategy {
        
        /// <inheritdoc />
        public string GenerateName(string name) {
            return name.ToLower();
        }

        /// <inheritdoc />
        public void WriteName(string name, IDataWriter writer) {
            writer.WriteString(name.ToLower());
        }

        /// <inheritdoc />
        public Task WriteNameAsync(string name, IDataWriter writer) {
            return writer.WriteStringAsync(name.ToLower());
        }
    }
}