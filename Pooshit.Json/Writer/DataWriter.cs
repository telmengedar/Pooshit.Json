using System.IO;
using System.Threading.Tasks;

namespace Pooshit.Json.Writer;

/// <inheritdoc />
public class DataWriter : IDataWriter {
    readonly TextWriter writer;
        
    /// <summary>
    /// creates a new <see cref="DataWriter"/>
    /// </summary>
    /// <param name="writer">target to write data to</param>
    public DataWriter(TextWriter writer) {
        this.writer = writer;
    }

    /// <inheritdoc />
    public void WriteCharacter(char character) {
        writer.Write(character);
    }

    /// <inheritdoc />
    public void WriteString(string data) {
        writer.Write(data);
    }

    /// <inheritdoc />
    public Task WriteCharacterAsync(char character) {
        return writer.WriteAsync(character);
    }

    /// <inheritdoc />
    public Task WriteStringAsync(string data) {
        return writer.WriteAsync(data);
    }
}