using Pooshit.Reflection;

namespace Json.Tests.Data;

[ReflectType]
public class GetOnlyIdElementContainerData {
    public string Name { get; set; }

    public ComputedIdData[] Elements { get; set; }
}
