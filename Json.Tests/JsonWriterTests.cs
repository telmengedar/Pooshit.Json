using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Json.Tests.Data;
using NUnit.Framework;
using Pooshit.Json;
using Pooshit.Json.Writer.Naming;

namespace Json.Tests;

[TestFixture, Parallelizable]
public class JsonWriterTests {
    async IAsyncEnumerable<string> GenerateAsyncEnumerable() {
        yield return "hello";
        await Task.Yield();
        yield return "bamm";
        yield return "bumm";
    }
    
    
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
    public void StripBinary() {
        string result = Pooshit.Json.Json.WriteString(new byte[] { 1, 2, 3 }, new() {
            ByteArrayBehavior = ByteArrayBehavior.Strip
        });
        Assert.AreEqual("null", result);
    }

    [Test, Parallelizable]
    public void StripBinaryInDictionary() {
        string result = Pooshit.Json.Json.WriteString(new Dictionary<string, object> {
            ["prop"]=new byte[] { 1, 2, 3 }
        }, new() {
            ByteArrayBehavior = ByteArrayBehavior.Strip
        });
        Assert.AreEqual("{\"prop\":null}", result);
    }

    [Test, Parallelizable]
    public void StripBinaryInObject() {
        string result = Pooshit.Json.Json.WriteString(new TestData {
            Binary= [1, 2, 3]
        }, new() {
            ByteArrayBehavior = ByteArrayBehavior.Strip
        });
        Assert.That(result.Contains("\"Binary\":null"));
    }

    [Test, Parallelizable]
    public void WriteBinaryAsBase64() {
        string result = Pooshit.Json.Json.WriteString(new byte[] { 1, 2, 3 }, new() {
            ByteArrayBehavior = ByteArrayBehavior.Base64
        });
        Assert.AreEqual("\"AQID\"", result);
    }

    [Test, Parallelizable]
    public void ReadBase64AsBinaryInObject() {
        string structure = Pooshit.Json.Json.WriteString(new TestData {
            Binary = [1, 2, 3]
        }, new() {
            NamingStrategy = new CamelCaseNamingStrategy(),
            ByteArrayBehavior = ByteArrayBehavior.Base64
        });

        Assert.That(structure.Contains("\"AQID\""));
        TestData testObject = Pooshit.Json.Json.Read<TestData>(structure);
        Assert.That(testObject.Binary, Is.EquivalentTo(new byte[] { 1, 2, 3 }));
    }

    [Test, Parallelizable]
    public void ReadBase64AsBinary() {
        string b64 = Pooshit.Json.Json.WriteString(new byte[] { 1, 2, 3 }, new() {
            ByteArrayBehavior = ByteArrayBehavior.Base64
        });

        byte[] byteArray = Pooshit.Json.Json.Read<byte[]>(b64);
        Assert.That(byteArray, Is.EquivalentTo(new byte[] { 1, 2, 3 }));
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
                                                                                      },
                                                                         SubComplex = new() {
                                                                                                ["test"] = new() {
                                                                                                                     ["my"] = new() { String = "little" }
                                                                                                                 }
                                                                                            }
                                                                     });
            
        ArbitraryDic readback = Pooshit.Json.Json.Read<ArbitraryDic>(data);
        Assert.NotNull(readback);
        Assert.NotNull(readback.Floats);
        Assert.NotNull(readback.Subs);
        Assert.NotNull(readback.SubComplex);
        Assert.That(readback.Subs.ContainsKey("test"));
        Assert.AreEqual(7.0f, readback.Floats["hello"]);
        Assert.AreEqual("gangolf", readback.Subs["test"]["name"]);
        Assert.That(readback.SubComplex.ContainsKey("test"));
        Assert.That(readback.SubComplex["test"].ContainsKey("my"));
        Assert.AreEqual("little", readback.SubComplex["test"]["my"].String);
    }

    [Test, Parallelizable]
    public async Task WriteObjectAsyncNoBOM() {
        MemoryStream ms = new();
        await Pooshit.Json.Json.WriteAsync(new TestData {
                                                            String = "lol"
                                                        }, ms, JsonOptions.RestApi);

        byte[] data = ms.ToArray();
        Assert.AreEqual(123, data[0]);
    }
    
    [Test, Parallelizable]
    public async Task WriteEmptyAsyncEnumerable() {
        MemoryStream ms = new();
        await Pooshit.Json.Json.WriteAsync(AsyncEnumerable.Empty<int>(), ms, JsonOptions.RestApi);

        byte[] data = ms.ToArray();
        CollectionAssert.AreEqual("[]"u8.ToArray(), data);
    }

    [Test, Parallelizable]
    public async Task WriteFilledAsyncEnumerable() {
        MemoryStream ms = new();
        await Pooshit.Json.Json.WriteAsync(new[]{3,8,0,1}.ToAsyncEnumerable(), ms, JsonOptions.RestApi);

        byte[] data = ms.ToArray();
        CollectionAssert.AreEqual("[3,8,0,1]"u8.ToArray(), data);
    }

    [Test, Parallelizable]
    public async Task WriteAsyncEnumerableFromMethod() {
        MemoryStream ms = new();
        await Pooshit.Json.Json.WriteAsync(GenerateAsyncEnumerable(), ms, JsonOptions.RestApi);

        byte[] data = ms.ToArray();
        CollectionAssert.AreEqual("[\"hello\",\"bamm\",\"bumm\"]"u8.ToArray(), data);
    }

    [Test, Parallelizable]
    public async Task WriteDictionaryUsesOptions() {
        Dictionary<string, object> dic = new() {
            ["Null"] = null,
            ["CamelCase"] = "hello"
        };

        string result = Pooshit.Json.Json.WriteString(dic, JsonOptions.RestApi);
        Assert.That(result, Is.EqualTo("{\"camelCase\":\"hello\"}"));
    }

    [Test, Parallelizable]
    public async Task CamelCaseLowerCamelToo()
    {
        Dictionary<string, object> dic = new() {
            ["camelCase"] = "hello"
        };
        
        string result = Pooshit.Json.Json.WriteString(dic, JsonOptions.RestApi);
        Assert.That(result, Is.EqualTo("{\"camelCase\":\"hello\"}"));
    }
}