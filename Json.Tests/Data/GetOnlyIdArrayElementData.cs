using Pooshit.Json;
using Pooshit.Reflection;

namespace Json.Tests.Data;

[ReflectType]
public class GetOnlyIdArrayElementData {
    public string Label { get; set; }

    [JsonWrite]
    public long Id => 918273645L;
}
