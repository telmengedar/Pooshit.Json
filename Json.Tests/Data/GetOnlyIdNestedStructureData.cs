using Pooshit.Reflection;

namespace Json.Tests.Data;

[ReflectType]
public class GetOnlyIdNestedStructureData {
    public string Title { get; set; }

    public GetOnlyIdElementContainerData Container { get; set; }
}
