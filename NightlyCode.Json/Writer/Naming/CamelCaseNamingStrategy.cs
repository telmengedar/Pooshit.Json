using System.Threading.Tasks;

namespace NightlyCode.Json.Writer.Naming {
    
    /// <summary>
    /// converts starting uppercase characters to lowercase
    /// </summary>
    public class CamelCaseNamingStrategy : INamingStrategy {
        
        string ModifyName(string name) {
            int index = -1;
            for (int i = 0; i < name.Length; ++i) {
                if (!char.IsLower(name[i])) 
                    continue;
                index = i;
                break;
            }

            if (index <= 0)
                return name.ToLower();
            return $"{name.Substring(0, index).ToLower()}{name.Substring(index, name.Length - index)}";
        }

        /// <inheritdoc />
        public void WriteName(string name, IDataWriter writer) {
            writer.WriteString(ModifyName(name));
        }

        /// <inheritdoc />
        public Task WriteNameAsync(string name, IDataWriter writer) {
            return writer.WriteStringAsync(ModifyName(name));
        }
    }
}