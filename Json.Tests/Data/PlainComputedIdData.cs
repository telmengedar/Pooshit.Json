using Pooshit.Json;

namespace Json.Tests.Data;

public class PlainComputedIdData {
    public string Job { get; set; }

    [JsonWrite]
    public long Id => 918273645L;
}
