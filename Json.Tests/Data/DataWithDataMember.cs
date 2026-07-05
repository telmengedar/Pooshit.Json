using System.Runtime.Serialization;

namespace Json.Tests.Data;

/// <summary>
/// test data class where a property carries a [DataMember(Name=...)] override
/// </summary>
public class DataWithDataMember {
    [DataMember(Name = "x")]
    public int Value { get; set; }

    public string Label { get; set; }
}
