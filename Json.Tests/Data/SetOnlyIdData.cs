using Pooshit.Reflection;

namespace Json.Tests.Data;

[ReflectType]
public class SetOnlyIdData {
    long id;

    public string Job { get; set; }

    public long Id {
        set => id = value;
    }
}
