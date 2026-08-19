using Pooshit.Json;

namespace Json.Tests.Data;

public class JsonWriteSetOnlyIdData {
    long id;

    public string Job { get; set; }

    [JsonWrite]
    public long Id {
        set => id = value;
    }
}
