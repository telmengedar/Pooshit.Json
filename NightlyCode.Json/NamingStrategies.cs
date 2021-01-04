using NightlyCode.Json.Writer;
using NightlyCode.Json.Writer.Naming;

namespace NightlyCode.Json {
    
    /// <summary>
    /// well known naming strategies which can be used with <see cref="JsonOptions"/>
    /// </summary>
    public static class NamingStrategies {

        /// <summary>
        /// writes property names as they are
        /// </summary>
        public static INamingStrategy None => new DefaultNamingStrategy();

        /// <summary>
        /// converts starting uppercase characters to lowercase
        /// </summary>
        public static INamingStrategy CamelCase => new CamelCaseNamingStrategy();

        /// <summary>
        /// writes property names in all lowercase
        /// </summary>
        public static INamingStrategy LowerCase => new LowerCaseNamingStrategy();

    }
}