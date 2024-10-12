using System.Threading.Tasks;

namespace Pooshit.Json.Reader;

/// <summary>
/// reads data for json deserialization
/// </summary>
public interface IDataReader {
        
    /// <summary>
    /// reads a character
    /// </summary>
    /// <returns>character retrieved from stream. if \0 is returned no more characters are available</returns>
    char ReadCharacter();

    /// <summary>
    /// reads multiple characters
    /// </summary>
    /// <param name="characters">target to read characters to</param>
    void ReadCharacters(char[] characters);

    /// <summary>
    /// reads a character
    /// </summary>
    /// <returns>character retrieved from stream. if \0 is returned no more characters are available</returns>
    Task<char> ReadCharacterAsync();

    /// <summary>
    /// reads multiple characters
    /// </summary>
    /// <param name="characters">target to read characters to</param>
    /// <param name="count">number of characters to read</param>
    Task<int> ReadCharactersAsync(char[] characters, int count);

}