using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Json.Tests.Data;
using NUnit.Framework;

namespace Json.Tests;

[TestFixture, Parallelizable]
public class JsonWriterTests {

    [TestCase(1, "1")]
    [TestCase(8L, "8")]
    [TestCase(9.98, "9.98")]
    [TestCase(11.13f, "11.13")]
    [TestCase(true, "true")]
    [TestCase(false, "false")]
    [TestCase(null, "null")]
    [TestCase("1234567890", "\"1234567890\"")]
    [Parallelizable]
    public void WriteValue(object data, string expected) {
        string result = Pooshit.Json.Json.WriteString(data);
        Assert.AreEqual(expected, result);
    }

    [Test, Parallelizable]
    public void WriteDateTime() {
        string result = Pooshit.Json.Json.WriteString(new DateTime(2020, 12, 31));
        Assert.AreEqual("\"2020-12-31T00:00:00.0000000\"", result);
    }

    [Test, Parallelizable]
    public async Task WriteDateTimeAsync() {
        string result = await Pooshit.Json.Json.WriteStringAsync(new DateTime(2020, 12, 31));
        Assert.AreEqual("\"2020-12-31T00:00:00.0000000\"", result);
    }

    [Test, Parallelizable]
    public void WriteTimeSpan() {
        string result = Pooshit.Json.Json.WriteString(new TimeSpan(4, 34, 22));
        Assert.AreEqual("\"04:34:22\"", result);
    }

    [Test, Parallelizable]
    public async Task WriteTimeSpanAsync() {
        string result = await Pooshit.Json.Json.WriteStringAsync(new TimeSpan(4, 34, 22));
        Assert.AreEqual("\"04:34:22\"", result);
    }

    [TestCase(1, "1")]
    [TestCase(8L, "8")]
    [TestCase(9.98, "9.98")]
    [TestCase(11.13f, "11.13")]
    [TestCase(true, "true")]
    [TestCase(false, "false")]
    [TestCase(null, "null")]
    [Parallelizable]
    public async Task WriteValueAsync(object data, string expected) {
        string result = await Pooshit.Json.Json.WriteStringAsync(data);
        Assert.AreEqual(expected, result);
    }

    [Test, Parallelizable]
    public void WriteDecimal() {
        string result = Pooshit.Json.Json.WriteString(13.44m);
        Assert.AreEqual("13.44", result);
    }

    [Test, Parallelizable]
    public async Task WriteDecimalAsync() {
        string result = await Pooshit.Json.Json.WriteStringAsync(13.44m);
        Assert.AreEqual("13.44", result);
    }

    [Test, Parallelizable]
    public void WriteGuid() {
        string result = Pooshit.Json.Json.WriteString(Guid.Empty);
        Assert.AreEqual($"\"{Guid.Empty}\"", result);
    }

    [Test, Parallelizable]
    public async Task WriteGuidAsync() {
        string result = await Pooshit.Json.Json.WriteStringAsync(Guid.Empty);
        Assert.AreEqual($"\"{Guid.Empty}\"", result);
    }

    [Test, Parallelizable]
    public void WriteObject() {
        string result = Pooshit.Json.Json.WriteString(new TestData {
                                                                       Decimal = 0.22m,
                                                                       Long = 92,
                                                                       String = "Hello",
                                                                       Array = [1, 5, 4, 3, 3],
                                                                       ChildTestData = new() {
                                                                                                 String = "bums\"bom"
                                                                                             }
                                                                   });

        TestData testdata = Pooshit.Json.Json.Read<TestData>(result);
        Assert.AreEqual(0.22m, testdata.Decimal);
        Assert.AreEqual(92, testdata.Long);
        Assert.AreEqual("Hello", testdata.String);
        Assert.That(new[] {1, 5, 4, 3, 3}.SequenceEqual(testdata.Array));
        Assert.NotNull(testdata.ChildTestData);
        Assert.AreEqual("bums\"bom", testdata.ChildTestData.String);
    }

    [Test, Parallelizable]
    public async Task WriteObjectAsync() {
        string result = await Pooshit.Json.Json.WriteStringAsync(new TestData {
                                                                                  Decimal = 0.22m,
                                                                                  Long = 92,
                                                                                  String = "Hello",
                                                                                  Array = [1, 5, 4, 3, 3],
                                                                                  ChildTestData = new() {
                                                                                                            String = "bums\"bom"
                                                                                                        }
                                                                              });

        TestData testdata = await Pooshit.Json.Json.ReadAsync<TestData>(result);
        Assert.AreEqual(0.22m, testdata.Decimal);
        Assert.AreEqual(92, testdata.Long);
        Assert.AreEqual("Hello", testdata.String);
        Assert.That(new[] {1, 5, 4, 3, 3}.SequenceEqual(testdata.Array));
        Assert.NotNull(testdata.ChildTestData);
        Assert.AreEqual("bums\"bom", testdata.ChildTestData.String);
    }

    [Test, Parallelizable]
    public void WriteEnum() {
        string result = Pooshit.Json.Json.WriteString(DayOfWeek.Tuesday);
        Assert.AreEqual("2", result);
    }

    [Test, Parallelizable]
    public async Task WriteEnumAsync() {
        string result = await Pooshit.Json.Json.WriteStringAsync(DayOfWeek.Tuesday);
        Assert.AreEqual("2", result);
    }

    [Test, Parallelizable]
    public void WriteEnumString() {
        string result = Pooshit.Json.Json.WriteString(DayOfWeek.Tuesday, new() {
                                                                                   WriteEnumsAsStrings = true 
                                                                               });
        Assert.AreEqual("\"Tuesday\"", result);
    }

    [Test, Parallelizable]
    public async Task WriteEnumStringAsync() {
        string result = await Pooshit.Json.Json.WriteStringAsync(DayOfWeek.Tuesday, new() {
                                                                                              WriteEnumsAsStrings = true
                                                                                          });
        Assert.AreEqual("\"Tuesday\"", result);
    }

    [Test, Parallelizable]
    public void WriteNullableNull() {
        int? data = null;
        // ReSharper disable once ExpressionIsAlwaysNull
        string result = Pooshit.Json.Json.WriteString(data);
        Assert.AreEqual("null", result);
    }

    [Test, Parallelizable]
    public async Task WriteNullableNullAsync() {
        int? data = null;
        // ReSharper disable once ExpressionIsAlwaysNull
        string result = await Pooshit.Json.Json.WriteStringAsync(data);
        Assert.AreEqual("null", result);
    }

    [Test, Parallelizable]
    public void WriteNullableValue() {
        int? data = 92;
        string result = Pooshit.Json.Json.WriteString(data);
        Assert.AreEqual("92", result);
    }
        
    [Test, Parallelizable]
    public async Task WriteNullableValueAsync() {
        int? data = 92;
        string result = await Pooshit.Json.Json.WriteStringAsync(data);
        Assert.AreEqual("92", result);
    }

    [Test, Parallelizable]
    public void WriteEscapedString() {
        string result = Pooshit.Json.Json.WriteString("Hello\nNext Line\n\ttabbed content");
        Assert.AreEqual("\"Hello\\nNext Line\\n\\ttabbed content\"",result);
    }
        
    [Test, Parallelizable]
    public async Task WriteEscapedStringAsync() {
        string result = await Pooshit.Json.Json.WriteStringAsync("Hello\nNext Line\n\ttabbed content");
        Assert.AreEqual("\"Hello\\nNext Line\\n\\ttabbed content\"",result);
    }

    [Test, Parallelizable]
    public void WriteDataWithIndexer() {
        string result = Pooshit.Json.Json.WriteString(new DataWithIndexer());
    }

    [Test, Parallelizable]
    public void WriteList() {
        string result = Pooshit.Json.Json.WriteString(new List<object> {1, 2, 3, 4, 5});
        Assert.AreEqual("[1,2,3,4,5]", result);
    }
        
    [Test, Parallelizable]
    public async Task WriteListAsync() {
        string result = await Pooshit.Json.Json.WriteStringAsync(new List<object> {1, 2, 3, 4, 5});
        Assert.AreEqual("[1,2,3,4,5]", result);
    }
        
    [Test, Parallelizable]
    public void IgnoreAttribute() {
        string result = Pooshit.Json.Json.WriteString(new DataWithIgnoredProperties());
        Assert.AreEqual("{\"Visible\":0}", result);
    }
        
    [Test, Parallelizable]
    public async Task IgnoreAttributeAsync() {
        string result = await Pooshit.Json.Json.WriteStringAsync(new DataWithIgnoredProperties());
        Assert.AreEqual("{\"Visible\":0}", result);
    }

    [Test, Parallelizable]
    public void WriteNaNasNull() {
        string result = Pooshit.Json.Json.WriteString(double.NaN);
        Assert.AreEqual("null", result);
    }
        
    [Test, Parallelizable]
    public void WriteInfinityasNull() {
        string result = Pooshit.Json.Json.WriteString(double.PositiveInfinity);
        Assert.AreEqual("null", result);
    }
        
    [Test, Parallelizable]
    public void WriteNegativeInfinityasNull() {
        string result = Pooshit.Json.Json.WriteString(double.NegativeInfinity);
        Assert.AreEqual("null", result);
    }

    [Test, Parallelizable]
    public void WriteAndReadBackDoubleNaN() {
        string result = Pooshit.Json.Json.WriteString(double.NaN);
        double value = Pooshit.Json.Json.Read<double>(result);
    }
    
    [Test, Parallelizable]
    public void WriteAndReadCustomDictionaries() {
        string data = Pooshit.Json.Json.WriteString(new ArbitraryDic {
                                                                         Floats = new() {
                                                                                            ["hello"] = 7.0f
                                                                                        },
                                                                         Subs = new() {
                                                                                          ["test"] = new() {
                                                                                                               ["name"] = "gangolf"
                                                                                                           }
                                                                                      }
                                                                     });
            
        ArbitraryDic readback = Pooshit.Json.Json.Read<ArbitraryDic>(data);
        Assert.NotNull(readback);
        Assert.NotNull(readback.Floats);
        Assert.NotNull(readback.Subs);
        Assert.That(readback.Subs.ContainsKey("test"));
        Assert.AreEqual(7.0f, readback.Floats["hello"]);
        Assert.AreEqual("gangolf", readback.Subs["test"]["name"]);
    }
}