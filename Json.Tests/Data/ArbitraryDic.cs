using System.Collections.Generic;
using Pooshit.Reflection;

namespace Json.Tests.Data;

[ReflectType]
public class ArbitraryDic {
    public Dictionary<string, float> Floats { get; set; }
    public Dictionary<string, Dictionary<string, object>> Subs { get; set; }

    public Dictionary<string, Dictionary<string, TestData>> SubComplex { get; set; }
}