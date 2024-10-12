using System.Runtime.Serialization;
using Pooshit.Reflection;

namespace Json.Tests.Data;

[ReflectType]
public class SnakeData {
        
    [DataMember(Name="over_the_top")]
    public int OverTheTop { get; set; }

    public int Bum => 7;
}