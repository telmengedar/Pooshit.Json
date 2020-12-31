using NightlyCode.Json.Writer;

namespace NightlyCode.Json {
    
    /// <summary>
    /// well known naming strategies which can be used with <see cref="JsonOptions"/>
    /// </summary>
    public static class NamingStrategies {

        /// <summary>
        /// writes property names as they are
        /// </summary>
        /// <param name="name">name to write</param>
        /// <param name="writer">writer to write property name to</param>
        public static void None(string name, IDataWriter writer) {
            writer.WriteString(name);
        }
        
        /// <summary>
        /// converts starting uppercase characters to lowercase
        /// </summary>
        /// <param name="name">name to convert</param>
        /// <param name="writer">writer to write result to</param>
        public static void CamelCase(string name, IDataWriter writer) {
            int index = -1;
            for (int i = 0; i < name.Length; ++i) {
                if (!char.IsLower(name[i])) 
                    continue;
                index = i;
                break;
            }

            if (index <= 0)
                writer.WriteString(name.ToLower());
            else {
                writer.WriteString(name.Substring(0, index).ToLower());
                writer.WriteString(name.Substring(index, name.Length - index));
            }
        }

        /// <summary>
        /// writes property names in all lowercase
        /// </summary>
        /// <param name="name">property name to convert</param>
        /// <param name="writer">writer to write result to</param>
        public static void LowerCase(string name, IDataWriter writer) {
            writer.WriteString(name.ToLower());
        }
    }
}