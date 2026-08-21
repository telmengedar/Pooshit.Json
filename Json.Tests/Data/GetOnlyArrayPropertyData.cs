using Pooshit.Reflection;

namespace Json.Tests.Data;

[ReflectType]
public class GetOnlyArrayPropertyData {
    public object[] Items { get; } = ["a", "b"];
}
