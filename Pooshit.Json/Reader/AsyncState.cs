namespace Pooshit.Json.Reader {
    
    /// <summary>
    /// state used when reading json in a async task
    /// </summary>
    public class AsyncState {
        
        /// <summary>
        /// last character read from reader
        /// </summary>
        public char State { get; set; }
    }
}