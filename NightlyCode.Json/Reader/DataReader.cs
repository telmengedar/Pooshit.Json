using System;
using System.IO;

namespace NightlyCode.Json.Reader {
    /// <inheritdoc />
    public class DataReader : IDataReader {
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

        public void ReadCharacters(char[] characters) {
            reader.ReadBlock(characters, 0, characters.Length);
        }
    }
}