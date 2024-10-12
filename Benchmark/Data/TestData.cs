using System;
using Pooshit.Reflection;

namespace Benchmark.Data;

[ReflectType]
public class TestData {
    public long Long { get; set; }
    public TestData ChildTestData { get; set; }
    public string String { get; set; }
    public decimal Decimal { get; set; }
    public int[] Array { get; set; }

    public Guid Guid { get; set; }
}