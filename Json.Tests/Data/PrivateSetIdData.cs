using Pooshit.Reflection;

namespace Json.Tests.Data;

[ReflectType]
public class PrivateSetIdData {

    public PrivateSetIdData() {
    }

    public PrivateSetIdData(long id) => Id = id;

    public string Job { get; set; }

    public long Id { get; private set; }
}
