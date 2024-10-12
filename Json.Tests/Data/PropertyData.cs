using Pooshit.Reflection;

namespace Json.Tests.Data;

[ReflectType]
public class PropertyData {
    public long ID { get; set; }
    public long MagicCamel { get; set; }
}