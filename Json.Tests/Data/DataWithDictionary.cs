using System.Collections.Generic;
using Pooshit.Reflection;

namespace Json.Tests.Data;

[ReflectType]
public class DataWithDictionary {
    public IDictionary<string,object> Dictionary { get; set; }
}