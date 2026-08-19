using Pooshit.Json;

namespace Json.Tests.Data;

public class PlainJsonWritePrivateSetIdData {

    public PlainJsonWritePrivateSetIdData() {
    }

    public PlainJsonWritePrivateSetIdData(long id) => Id = id;

    public string Job { get; set; }

    [JsonWrite]
    public long Id { get; private set; }
}
