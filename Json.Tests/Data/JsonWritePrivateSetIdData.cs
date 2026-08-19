using Pooshit.Json;
using Pooshit.Reflection;

namespace Json.Tests.Data;

[ReflectType]
public class JsonWritePrivateSetIdData {

    public JsonWritePrivateSetIdData() {
    }

    public JsonWritePrivateSetIdData(long id) => Id = id;

    public string Job { get; set; }

    [JsonWrite]
    public long Id { get; private set; }
}
