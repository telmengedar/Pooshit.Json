using System.Runtime.Serialization;

namespace Json.Tests.Data {
    public class SnakeData {
        
        [DataMember(Name="over_the_top")]
        public int OverTheTop { get; set; }
    }
}