namespace NightlyCode.Json.Tokens {
    
    /// <summary>
    /// token in jpath
    /// </summary>
    public class JPathToken {
        
        /// <summary>
        /// index for collections
        /// </summary>
        public int? Index { get; set; }
        
        /// <summary>
        /// name of property
        /// </summary>
        public string Property { get; set; }
    }
}