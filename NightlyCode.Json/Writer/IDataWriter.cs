using System;

namespace NightlyCode.Json.Writer {
    
    /// <summary>
    /// used to write json data to a target
    /// </summary>
    public interface IDataWriter {
        
        /// <summary>
        /// writes a character
        /// </summary>
        /// <param name="character">character to write</param>
        void WriteCharacter(char character);

        /// <summary>
        /// writes a string
        /// </summary>
        /// <param name="data">string to write</param>
        void WriteString(string data);
    }
}