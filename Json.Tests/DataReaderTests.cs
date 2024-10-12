using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using Pooshit.Json.Reader;

namespace Json.Tests;

[TestFixture, Parallelizable]
public class DataReaderTests {

    [Test, Parallelizable]
    public async Task ReadAsyncEndOfStream() {
        TextReader text = new StringReader("1");
        DataReader reader = new(text);
        await reader.ReadCharacterAsync();
        char data = await reader.ReadCharacterAsync();
        Assert.AreEqual('\0', data);
    }
}