using NightlyCode.Json.Writer;

namespace NightlyCode.Json {
    
    /// <summary>
    /// options when writing json
    /// </summary>
    public struct JsonOptions {
        
        /// <summary>
        /// strategy used to write property names
        /// </summary>
        /// <remarks>
        /// this is only used for object properties, not for dictionary keys
        /// </remarks>
        public INamingStrategy NamingStrategy { get; set; }
        
        /// <summary>
        /// specifies whether to exclude properties which evaluate to null
        /// </summary>
        public bool ExcludeNullProperties { get; set; }

        /// <summary>
        /// determines whether to write the string representation of enum values
        /// if false the underlying number value is written
        /// </summary>
        public bool WriteEnumsAsStrings { get; set; }
        
        /// <summary>
        /// default settings used when writing json
        /// </summary>
        public static JsonOptions Default => new JsonOptions {
            NamingStrategy = NamingStrategies.None,
            ExcludeNullProperties = true
        };
    }
}