using Pooshit.Reflection;

namespace Json.Tests.Data;

[ReflectType]
public class ComputedIdData {
    public string Job { get; set; }

    public long Id => 918273645L;
}
