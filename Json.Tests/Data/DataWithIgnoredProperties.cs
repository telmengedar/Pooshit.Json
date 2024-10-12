using System.Runtime.Serialization;
using Pooshit.Reflection;

namespace Json.Tests.Data;

[ReflectType]
public class DataWithIgnoredProperties {
    public int Visible { get; set; }
        
    [IgnoreDataMember]
    public int Invisible { get; set; }
}