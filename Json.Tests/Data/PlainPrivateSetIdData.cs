namespace Json.Tests.Data;

public class PlainPrivateSetIdData {
    public PlainPrivateSetIdData() {
    }

    public PlainPrivateSetIdData(long id) => Id = id;

    public string Job { get; set; }

    public long Id { get; private set; }
}
