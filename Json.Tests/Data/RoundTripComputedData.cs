using Pooshit.Reflection;

namespace Json.Tests.Data;

[ReflectType]
public class RoundTripComputedData {
    public string Before { get; set; }

    public long Id => 918273645L;

    public string After { get; set; }
}
