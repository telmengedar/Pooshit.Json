using System.Threading.Tasks;

namespace Pooshit.Json.Writer.Naming {
    
    /// <summary>
    /// converts starting uppercase characters to lowercase
    /// </summary>
    public class CamelCaseNamingStrategy : INamingStrategy {
        
        /// <inheritdoc />
        public string GenerateName(string name) {
            int index = -1;
            for (int i = 0; i < name.Length; ++i) {
                if (!char.IsLower(name[i])) 
                    continue;
                index = i;
                break;
            }

            switch (index)
            {
                case -1:
                    return name.ToLower();
                case 0:
                    return name;
            }
            return $"{name.Substring(0, index).ToLower()}{name.Substring(index, name.Length - index)}";
        }

        /// <inheritdoc />
        public void WriteName(string name, IDataWriter writer) {
            writer.WriteString(GenerateName(name));
        }

        /// <inheritdoc />
        public Task WriteNameAsync(string name, IDataWriter writer) {
            return writer.WriteStringAsync(GenerateName(name));
        }
    }
}