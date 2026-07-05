using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
    public void WriteDictionaryUsesOptions() {
        Dictionary<string, object> dic = new() {
            ["Null"] = null,
            ["CamelCase"] = "hello"
        };

        string result = Pooshit.Json.Json.WriteString(dic, JsonOptions.RestApi);
        Assert.That(result, Is.EqualTo("{\"null\":null,\"camelCase\":\"hello\"}"), "Explicit null dict entry must survive under RestApi; 'Null' becomes 'null' via CamelCase strategy");
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
    
    [Test, Parallelizable]
    public async Task WriteDictionaryUsesOptionsAsync() {
        Dictionary<string, object> dic = new() {
            ["Null"] = null,
            ["CamelCase"] = "hello"
        };

        string result = await Pooshit.Json.Json.WriteStringAsync(dic, JsonOptions.RestApi);
        Assert.That(result, Is.EqualTo("{\"null\":null,\"camelCase\":\"hello\"}"), "Explicit null dict entry must survive under RestApi; 'Null' becomes 'null' via CamelCase strategy");
    }
    
    [Test, Parallelizable]
    public async Task CamelCaseLowerCamelTooAsync()
    {
        Dictionary<string, object> dic = new() {
            ["camelCase"] = "hello"
        };

        string result = await Pooshit.Json.Json.WriteStringAsync(dic, JsonOptions.RestApi);
        Assert.That(result, Is.EqualTo("{\"camelCase\":\"hello\"}"));
    }

    [Test, Parallelizable]
    public void DictExplicitNullPreservedDefault() {
        Dictionary<string, object> dict = new() { ["a"] = "v", ["b"] = null };
        string result = Pooshit.Json.Json.WriteString(dict, JsonOptions.Default);
        Assert.That(result, Is.EqualTo("{\"a\":\"v\",\"b\":null}"));
    }

    [Test, Parallelizable]
    public void DictExplicitNullPreservedCamel() {
        Dictionary<string, object> dict = new() { ["a"] = "v", ["b"] = null };
        string result = Pooshit.Json.Json.WriteString(dict, JsonOptions.Camel);
        Assert.That(result, Is.EqualTo("{\"a\":\"v\",\"b\":null}"));
    }

    [Test, Parallelizable]
    public void DictExplicitNullPreservedRestApi() {
        Dictionary<string, object> dict = new() { ["a"] = "v", ["b"] = null };
        string result = Pooshit.Json.Json.WriteString(dict, JsonOptions.RestApi);
        Assert.That(result, Is.EqualTo("{\"a\":\"v\",\"b\":null}"));
    }

    [Test, Parallelizable]
    public async Task DictExplicitNullPreservedDefaultAsync() {
        Dictionary<string, object> dict = new() { ["a"] = "v", ["b"] = null };
        string result = await Pooshit.Json.Json.WriteStringAsync(dict, JsonOptions.Default);
        Assert.That(result, Is.EqualTo("{\"a\":\"v\",\"b\":null}"));
    }

    [Test, Parallelizable]
    public async Task DictExplicitNullPreservedCamelAsync() {
        Dictionary<string, object> dict = new() { ["a"] = "v", ["b"] = null };
        string result = await Pooshit.Json.Json.WriteStringAsync(dict, JsonOptions.Camel);
        Assert.That(result, Is.EqualTo("{\"a\":\"v\",\"b\":null}"));
    }

    [Test, Parallelizable]
    public async Task DictExplicitNullPreservedRestApiAsync() {
        Dictionary<string, object> dict = new() { ["a"] = "v", ["b"] = null };
        string result = await Pooshit.Json.Json.WriteStringAsync(dict, JsonOptions.RestApi);
        Assert.That(result, Is.EqualTo("{\"a\":\"v\",\"b\":null}"));
    }

    [Test, Parallelizable]
    public void NestedDictExplicitNullPreserved() {
        Dictionary<string, object> dict = new() {
            ["outer"] = new Dictionary<string, object> { ["inner"] = null, ["val"] = "x" }
        };
        string result = Pooshit.Json.Json.WriteString(dict, JsonOptions.Default);
        Assert.That(result, Is.EqualTo("{\"outer\":{\"inner\":null,\"val\":\"x\"}}"));
    }

    [Test, Parallelizable]
    public void DictNullRoundTripSync() {
        string json = "{\"a\":\"v\",\"b\":null}";
        Dictionary<string, object> dict = Pooshit.Json.Json.Read<Dictionary<string, object>>(json);
        string result = Pooshit.Json.Json.WriteString(dict, JsonOptions.Default);
        Assert.That(result, Is.EqualTo("{\"a\":\"v\",\"b\":null}"));
    }

    [Test, Parallelizable]
    public async Task DictNullRoundTripAsync() {
        string json = "{\"a\":\"v\",\"b\":null}";
        Dictionary<string, object> dict = await Pooshit.Json.Json.ReadAsync<Dictionary<string, object>>(json);
        string result = await Pooshit.Json.Json.WriteStringAsync(dict, JsonOptions.Default);
        Assert.That(result, Is.EqualTo("{\"a\":\"v\",\"b\":null}"));
    }

    [Test, Parallelizable]
    public void ObjectNullPropertyExcludedUnderDefault() {
        string result = Pooshit.Json.Json.WriteString(new TestData { Long = 1 }, JsonOptions.Default);
        Assert.That(result, Does.Not.Contain("\"String\""));
        Assert.That(result, Does.Not.Contain("\"ChildTestData\""));
        Assert.That(result, Does.Contain("\"Long\":1"));
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Async parity — NaN / Infinity (#3343-A1)
    // ──────────────────────────────────────────────────────────────────────────

    [Test, Parallelizable]
    public async Task WriteDoubleNaNAsNullAsync() {
        string result = await Pooshit.Json.Json.WriteStringAsync(double.NaN);
        Assert.That(result, Is.EqualTo("null"));
    }

    [Test, Parallelizable]
    public async Task WriteDoublePositiveInfinityAsNullAsync() {
        string result = await Pooshit.Json.Json.WriteStringAsync(double.PositiveInfinity);
        Assert.That(result, Is.EqualTo("null"));
    }

    [Test, Parallelizable]
    public async Task WriteDoubleNegativeInfinityAsNullAsync() {
        string result = await Pooshit.Json.Json.WriteStringAsync(double.NegativeInfinity);
        Assert.That(result, Is.EqualTo("null"));
    }

    [Test, Parallelizable]
    public async Task WriteFloatNaNAsNullAsync() {
        string result = await Pooshit.Json.Json.WriteStringAsync(float.NaN);
        Assert.That(result, Is.EqualTo("null"));
    }

    [Test, Parallelizable]
    public async Task WriteFloatPositiveInfinityAsNullAsync() {
        string result = await Pooshit.Json.Json.WriteStringAsync(float.PositiveInfinity);
        Assert.That(result, Is.EqualTo("null"));
    }

    /// <summary>
    /// sync and async must agree for NaN
    /// </summary>
    [Test, Parallelizable]
    public async Task NaNSyncAsyncParity() {
        string sync = Pooshit.Json.Json.WriteString(double.NaN);
        string async_ = await Pooshit.Json.Json.WriteStringAsync(double.NaN);
        Assert.That(async_, Is.EqualTo(sync));
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Async parity — ByteArrayBehavior (#3343-A2)
    // ──────────────────────────────────────────────────────────────────────────

    [Test, Parallelizable]
    public async Task StripBinaryAsync() {
        string result = await Pooshit.Json.Json.WriteStringAsync(new byte[] { 1, 2, 3 }, new() {
            ByteArrayBehavior = ByteArrayBehavior.Strip
        });
        Assert.That(result, Is.EqualTo("null"));
    }

    [Test, Parallelizable]
    public async Task WriteBinaryAsBase64Async() {
        string result = await Pooshit.Json.Json.WriteStringAsync(new byte[] { 1, 2, 3 }, new() {
            ByteArrayBehavior = ByteArrayBehavior.Base64
        });
        Assert.That(result, Is.EqualTo("\"AQID\""));
    }

    [Test, Parallelizable]
    public async Task StripBinaryInDictionaryAsync() {
        string result = await Pooshit.Json.Json.WriteStringAsync(new Dictionary<string, object> {
            ["prop"] = new byte[] { 1, 2, 3 }
        }, new() {
            ByteArrayBehavior = ByteArrayBehavior.Strip
        });
        Assert.That(result, Is.EqualTo("{\"prop\":null}"));
    }

    /// <summary>
    /// sync and async must agree for Base64
    /// </summary>
    [Test, Parallelizable]
    public async Task ByteArrayBase64SyncAsyncParity() {
        byte[] data = [1, 2, 3];
        JsonOptions opts = new() { ByteArrayBehavior = ByteArrayBehavior.Base64 };
        string sync = Pooshit.Json.Json.WriteString(data, opts);
        string async_ = await Pooshit.Json.Json.WriteStringAsync(data, opts);
        Assert.That(async_, Is.EqualTo(sync));
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Async parity — DataMemberAttribute (#3343-A3)
    // ──────────────────────────────────────────────────────────────────────────

    [Test, Parallelizable]
    public void DataMemberNameOverrideSync() {
        string result = Pooshit.Json.Json.WriteString(new DataWithDataMember { Value = 7, Label = "hi" });
        Assert.That(result, Does.Contain("\"x\":7"));
        Assert.That(result, Does.Not.Contain("\"Value\""));
    }

    [Test, Parallelizable]
    public async Task DataMemberNameOverrideAsync() {
        string result = await Pooshit.Json.Json.WriteStringAsync(new DataWithDataMember { Value = 7, Label = "hi" });
        Assert.That(result, Does.Contain("\"x\":7"));
        Assert.That(result, Does.Not.Contain("\"Value\""));
    }

    /// <summary>
    /// sync and async must agree for DataMember name override
    /// </summary>
    [Test, Parallelizable]
    public async Task DataMemberSyncAsyncParity() {
        DataWithDataMember obj = new() { Value = 42, Label = "parity" };
        string sync = Pooshit.Json.Json.WriteString(obj);
        string async_ = await Pooshit.Json.Json.WriteStringAsync(obj);
        Assert.That(async_, Is.EqualTo(sync));
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Async parity — FormatOutput (#3343-A4)
    // ──────────────────────────────────────────────────────────────────────────

    [Test, Parallelizable]
    public async Task FormatOutputObjectAsync() {
        JsonOptions opts = new() { FormatOutput = true, ExcludeNullProperties = false };
        string result = await Pooshit.Json.Json.WriteStringAsync(new DataWithDataMember { Value = 1, Label = "l" }, opts);
        Assert.That(result, Does.Contain("\t"));
        Assert.That(result, Does.Contain("\n"));
    }

    [Test, Parallelizable]
    public async Task FormatOutputDictAsync() {
        JsonOptions opts = new() { FormatOutput = true };
        string result = await Pooshit.Json.Json.WriteStringAsync(new Dictionary<string, object> {
            ["a"] = 1,
            ["b"] = 2
        }, opts);
        Assert.That(result, Does.Contain("\t"));
        Assert.That(result, Does.Contain("\n"));
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Key escaping — JsonWriter (#3347)
    // ──────────────────────────────────────────────────────────────────────────

    [Test, Parallelizable]
    public void DictKeyWithQuoteEscapedSync() {
        string result = Pooshit.Json.Json.WriteString(new Dictionary<string, object> {
            ["ke\"y"] = "val"
        });
        // the key in the output must not contain a bare unescaped quote
        Assert.That(result, Is.EqualTo("{\"ke\\\"y\":\"val\"}"));
    }

    [Test, Parallelizable]
    public async Task DictKeyWithQuoteEscapedAsync() {
        string result = await Pooshit.Json.Json.WriteStringAsync(new Dictionary<string, object> {
            ["ke\"y"] = "val"
        });
        Assert.That(result, Is.EqualTo("{\"ke\\\"y\":\"val\"}"));
    }

    [Test, Parallelizable]
    public void DictKeyWithBackslashEscapedSync() {
        string result = Pooshit.Json.Json.WriteString(new Dictionary<string, object> {
            ["ke\\y"] = "val"
        });
        Assert.That(result, Is.EqualTo("{\"ke\\\\y\":\"val\"}"));
    }

    [Test, Parallelizable]
    public async Task DictKeyWithBackslashEscapedAsync() {
        string result = await Pooshit.Json.Json.WriteStringAsync(new Dictionary<string, object> {
            ["ke\\y"] = "val"
        });
        Assert.That(result, Is.EqualTo("{\"ke\\\\y\":\"val\"}"));
    }

    [Test, Parallelizable]
    public void DictKeyWithNewlineEscapedSync() {
        string result = Pooshit.Json.Json.WriteString(new Dictionary<string, object> {
            ["ke\ny"] = "val"
        });
        Assert.That(result, Is.EqualTo("{\"ke\\ny\":\"val\"}"));
    }

    [Test, Parallelizable]
    public async Task DictKeyWithNewlineEscapedAsync() {
        string result = await Pooshit.Json.Json.WriteStringAsync(new Dictionary<string, object> {
            ["ke\ny"] = "val"
        });
        Assert.That(result, Is.EqualTo("{\"ke\\ny\":\"val\"}"));
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Polish — sync dict indentation (#3351-C5)
    // ──────────────────────────────────────────────────────────────────────────

    [Test, Parallelizable]
    public void FormatOutputNestedDictNoIndentLeak() {
        JsonOptions opts = new() { FormatOutput = true };
        string result = Pooshit.Json.Json.WriteString(new Dictionary<string, object> {
            ["outer"] = new Dictionary<string, object> { ["inner"] = 1 }
        }, opts);
        // The closing brace of the outer dict must be at indentation level 0 (no leading tabs)
        string[] lines = result.Split('\n');
        string lastLine = lines[^1];
        Assert.That(lastLine, Is.EqualTo("}"), $"closing brace should be at level 0, got: '{lastLine}'");
    }

    [Test, Parallelizable]
    public async Task FormatOutputNestedDictNoIndentLeakAsync() {
        JsonOptions opts = new() { FormatOutput = true };
        string result = await Pooshit.Json.Json.WriteStringAsync(new Dictionary<string, object> {
            ["outer"] = new Dictionary<string, object> { ["inner"] = 1 }
        }, opts);
        string[] lines = result.Split('\n');
        string lastLine = lines[^1];
        Assert.That(lastLine, Is.EqualTo("}"), $"closing brace should be at level 0, got: '{lastLine}'");
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Polish — IPAddress in stringtypes (#3351-C6)
    // ──────────────────────────────────────────────────────────────────────────

    [Test, Parallelizable]
    public void IPAddressWritesAsQuotedString() {
        string result = Pooshit.Json.Json.WriteString(IPAddress.Loopback);
        Assert.That(result, Is.EqualTo("\"127.0.0.1\""));
    }

    [Test, Parallelizable]
    public async Task IPAddressWritesAsQuotedStringAsync() {
        string result = await Pooshit.Json.Json.WriteStringAsync(IPAddress.Loopback);
        Assert.That(result, Is.EqualTo("\"127.0.0.1\""));
    }
}