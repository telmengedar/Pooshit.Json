using System.Runtime.Serialization;
using Pooshit.Json;

namespace Json.Tests.Data;

public class JsonWriteDataMemberIdData {
    public string Job { get; set; }

    [JsonWrite, DataMember(Name = "customId")]
    public long Id => 918273645L;
}
