using System.Text;
using Pooshit.Json.Writer.Naming;

namespace Pooshit.Json;

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
    /// determines whether to include line breaks and indentation when writing json data
    /// </summary>
    public bool FormatOutput { get; set; }

    /// <summary>
    /// encoding to use when reading or writing json
    /// </summary>
    public Encoding Encoding { get; set; }

    /// <summary>
    /// removes byte arrays from output
    /// </summary>
    public ByteArrayBehavior ByteArrayBehavior { get; set; }
        
    /// <summary>
    /// default settings used when writing json
    /// </summary>
    public static JsonOptions Default => new() {
                                                   NamingStrategy = NamingStrategies.None,
                                                   ExcludeNullProperties = true,
                                                   Encoding = new UTF8Encoding(false)
                                               };
        
    /// <summary>
    /// default settings used when writing json
    /// </summary>
    public static JsonOptions Camel => new() {
                                                 NamingStrategy = NamingStrategies.CamelCase,
                                                 ExcludeNullProperties = true,
                                                 Encoding = new UTF8Encoding(false)
                                             };

    /// <summary>
    /// default settings used for rest apis
    /// </summary>
    /// <remarks>
    /// obviously these are are defaults for pooshit apis and others could
    /// totally have different defaults
    /// </remarks>
    public static JsonOptions RestApi => new() {
                                                   NamingStrategy = NamingStrategies.CamelCase,
                                                   ExcludeNullProperties = true,
                                                   Encoding = new UTF8Encoding(false),
                                                   WriteEnumsAsStrings = true
                                               };
}