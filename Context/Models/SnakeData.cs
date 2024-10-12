using System.Runtime.Serialization;

namespace Context.Models;

public class SnakeData {
        
    [DataMember(Name="over_the_top")]
    public int OverTheTop { get; set; }
}