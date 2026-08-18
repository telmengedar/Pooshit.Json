using Pooshit.Reflection;

namespace Json.Tests.Data;

[ReflectType]
public class InternalSetIdData {
    public InternalSetIdData() {
    }

    public InternalSetIdData(long id) => Id = id;

    public string Job { get; set; }

    public long Id { get; internal set; }
}
