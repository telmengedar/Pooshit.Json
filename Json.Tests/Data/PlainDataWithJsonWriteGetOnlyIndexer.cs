using Pooshit.Json;

namespace Json.Tests.Data;

public class PlainDataWithJsonWriteGetOnlyIndexer {
    public string Job { get; set; }

    [JsonWrite]
    public string this[int index] => "indexed";
}
