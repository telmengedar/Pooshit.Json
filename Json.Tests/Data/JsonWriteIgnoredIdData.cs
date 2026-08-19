using System.Runtime.Serialization;
using Pooshit.Json;

namespace Json.Tests.Data;

public class JsonWriteIgnoredIdData {
    public string Job { get; set; }

    [JsonWrite, IgnoreDataMember]
    public long Id => 918273645L;
}
