namespace Json.Tests.Data;

public class PlainInternalSetIdData {
    public PlainInternalSetIdData() {
    }

    public PlainInternalSetIdData(long id) => Id = id;

    public string Job { get; set; }

    public long Id { get; internal set; }
}
