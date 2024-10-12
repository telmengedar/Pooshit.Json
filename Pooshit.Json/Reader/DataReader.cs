using System.IO;
using System.Threading.Tasks;

namespace Pooshit.Json.Reader {

    /// <inheritdoc />
    public class DataReader : IDataReader {
        readonly char[] asyncbuffer = new char[1]; 
        readonly TextReader reader;

        /// <summary>
        /// creates a new <see cref="DataReader"/>
        /// </summary>
        /// <param name="reader">reader used to retrieve data</param>
        public DataReader(TextReader reader) {
            this.reader = reader;
        }

        /// <inheritdoc />
        public char ReadCharacter() {
            int result = reader.Read();
            if (result == -1)
                return '\0';
            return (char)result;
        }

        /// <inheritdoc />
        public void ReadCharacters(char[] characters) {
            reader.ReadBlock(characters, 0, characters.Length);
        }

        /// <inheritdoc />
        public async Task<char> ReadCharacterAsync() {
            int result=await ReadCharactersAsync(asyncbuffer, 1);
            if (result <= 0)
                return '\0';
            return asyncbuffer[0];
        }

        /// <inheritdoc />
        public Task<int> ReadCharactersAsync(char[] characters, int count) {
            return reader.ReadBlockAsync(characters, 0, count);
        }
    }
}