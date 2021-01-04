using System.Runtime.Serialization;

namespace Json.Tests.Data {
    public class DataWithIgnoredProperties {
        public int Visible { get; set; }
        
        [IgnoreDataMember]
        public int Invisible { get; set; }
    }
}