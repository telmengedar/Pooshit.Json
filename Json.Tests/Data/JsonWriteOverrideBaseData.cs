using Pooshit.Json;

namespace Json.Tests.Data;

public class JsonWriteOverrideBaseData {
    public string Job { get; set; }

    [JsonWrite]
    public virtual long Id => 918273645L;
}
