namespace Json.Tests.Data;

public class PlainDataWithGetOnlyIndexer {
    public string Job { get; set; }

    public string this[int index] => "indexed";
}
