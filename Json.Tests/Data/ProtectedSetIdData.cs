using Pooshit.Reflection;

namespace Json.Tests.Data;

[ReflectType]
public class ProtectedSetIdData {
    public ProtectedSetIdData() {
    }

    public ProtectedSetIdData(long id) => Id = id;

    public string Job { get; set; }

    public long Id { get; protected set; }
}
