using Pooshit.Reflection;

namespace Json.Tests.Data;

[ReflectType]
public class NestedArrayPropertyData {
    public object[] Items { get; set; }
}
