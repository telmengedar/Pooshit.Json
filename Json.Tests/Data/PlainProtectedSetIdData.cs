namespace Json.Tests.Data;

public class PlainProtectedSetIdData {
    public PlainProtectedSetIdData() {
    }

    public PlainProtectedSetIdData(long id) => Id = id;

    public string Job { get; set; }

    public long Id { get; protected set; }
}
