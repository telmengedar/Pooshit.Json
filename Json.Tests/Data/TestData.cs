using System;
using Pooshit.Reflection;

namespace Json.Tests.Data;

[ReflectType]
public class TestData {
    public long Long { get; set; }
    public TestData ChildTestData { get; set; }
    public string String { get; set; }
    public decimal Decimal { get; set; }
    public int[] Array { get; set; }

    public Guid Guid { get; set; }

    public DayOfWeek WeekDay { get; set; }

    public TestData[] ObjectArray { get; set; }

    public DateTime Date { get; set; }

    public byte[] Binary { get; set; }
}