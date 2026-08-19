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

    [Test, Parallelizable]
    public void DictKeyWithQuoteEscapedSync() {
        string result = Pooshit.Json.Json.WriteString(new Dictionary<string, object> {
            ["ke\"y"] = "val"
        });
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

    [Test, Parallelizable]
    public void FormatOutputNestedDictNoIndentLeak() {
        JsonOptions opts = new() { FormatOutput = true };
        string result = Pooshit.Json.Json.WriteString(new Dictionary<string, object> {
            ["outer"] = new Dictionary<string, object> { ["inner"] = 1 }
        }, opts);
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

    [Test, Parallelizable]
    public void Write_ComputedGetOnlyProperty_EmitsValue() {
        string result = Pooshit.Json.Json.WriteString(new ComputedIdData { Job = "Dev" });
        Assert.That(result, Does.Contain("\"Id\":918273645"));
    }

    [Test, Parallelizable]
    public async Task WriteAsync_ComputedGetOnlyProperty_EmitsValue() {
        string result = await Pooshit.Json.Json.WriteStringAsync(new ComputedIdData { Job = "Dev" });
        Assert.That(result, Does.Contain("\"Id\":918273645"));
    }

    [Test, Parallelizable]
    [Description("Accepted consequence of opt-in write eligibility (DiVoid #3348, design §10): anonymous types are uniformly get-only and cannot carry [JsonWrite], so they serialize to an empty object again.")]
    public void Write_AnonymousType_EmitsEmptyObject() {
        string result = Pooshit.Json.Json.WriteString(new { name = "gangolf", value = 42 });
        Assert.That(result, Is.EqualTo("{}"));
    }

    [Test, Parallelizable]
    [Description("Accepted consequence of opt-in write eligibility (DiVoid #3348, design §10): anonymous types are uniformly get-only and cannot carry [JsonWrite], so they serialize to an empty object again.")]
    public async Task WriteAsync_AnonymousType_EmitsEmptyObject() {
        string result = await Pooshit.Json.Json.WriteStringAsync(new { name = "gangolf", value = 42 });
        Assert.That(result, Is.EqualTo("{}"));
    }

    [Test, Parallelizable]
    [Description("Characterization guard: init-only properties emit on write. Read-side wire-tamperability of init is separate, tracked debt (DiVoid #8452) and is unaffected here.")]
    public void Write_InitOnlyProperty_Emits() {
        string result = Pooshit.Json.Json.WriteString(new InitIdData { Job = "Dev", Id = 918273645L });
        Assert.That(result, Does.Contain("\"Id\":918273645"));
    }

    [Test, Parallelizable]
    public void Write_SetOnlyProperty_StaysOmitted() {
        SetOnlyIdData data = new() { Job = "Dev", Id = 918273645L };
        string result = Pooshit.Json.Json.WriteString(data);
        Assert.That(result, Does.Not.Contain("\"Id\""));
    }

    [Test, Parallelizable]
    [Description("D6 (design §6.3): a private-set property on the source-generated model path no longer emits without [JsonWrite].")]
    public void Write_PrivateSetPropertyWithReflectType_StaysOmitted() {
        string result = Pooshit.Json.Json.WriteString(new PrivateSetIdData(918273645L) { Job = "Dev" });
        Assert.That(result, Does.Not.Contain("\"Id\""));
    }

    [Test, Parallelizable]
    [Description("Regression guard: a get-only indexer on a plain reflection DTO must not throw and must be omitted from output.")]
    public void Write_GetOnlyIndexerOnPlainDto_OmitsIndexerEmitsOtherProperties() {
        string result = Pooshit.Json.Json.WriteString(new PlainDataWithGetOnlyIndexer { Job = "Dev" });
        Assert.That(result, Is.EqualTo("{\"Job\":\"Dev\"}"));
    }

    [Test, Parallelizable]
    [Description("Async counterpart of the get-only indexer regression guard.")]
    public async Task WriteAsync_GetOnlyIndexerOnPlainDto_OmitsIndexerEmitsOtherProperties() {
        string result = await Pooshit.Json.Json.WriteStringAsync(new PlainDataWithGetOnlyIndexer { Job = "Dev" });
        Assert.That(result, Is.EqualTo("{\"Job\":\"Dev\"}"));
    }

    [Test, Parallelizable]
    [Description("Break 2 guard (design §17): a private-set property on the plain reflection path no longer emits without [JsonWrite]. There is no single 0.4.0 'before' for this shape (design §6.3) - the two model paths now agree on omission.")]
    public void Write_PrivateSetPropertyWithoutReflectType_StaysOmitted() {
        string result = Pooshit.Json.Json.WriteString(new PlainPrivateSetIdData(918273645L) { Job = "Dev" });
        Assert.That(result, Does.Not.Contain("\"Id\""));
    }

    [Test, Parallelizable]
    [Description("Async counterpart of the Break 2 guard (design §17).")]
    public async Task WriteAsync_PrivateSetPropertyWithoutReflectType_StaysOmitted() {
        string result = await Pooshit.Json.Json.WriteStringAsync(new PlainPrivateSetIdData(918273645L) { Job = "Dev" });
        Assert.That(result, Does.Not.Contain("\"Id\""));
    }

    [Test, Parallelizable]
    [Description("S3: a get-only property without [JsonWrite] stays omitted on the source-generated model path.")]
    public void Write_GetOnlyPropertyWithReflectType_StaysOmitted() {
        string result = Pooshit.Json.Json.WriteString(new GetOnlyIdData { Job = "Dev" });
        Assert.That(result, Does.Not.Contain("\"Id\""));
    }

    [Test, Parallelizable]
    [Description("Async counterpart of S3 on the source-generated model path.")]
    public async Task WriteAsync_GetOnlyPropertyWithReflectType_StaysOmitted() {
        string result = await Pooshit.Json.Json.WriteStringAsync(new GetOnlyIdData { Job = "Dev" });
        Assert.That(result, Does.Not.Contain("\"Id\""));
    }

    [Test, Parallelizable]
    [Description("S3: a get-only property without [JsonWrite] stays omitted on the plain reflection model path.")]
    public void Write_GetOnlyPropertyWithoutReflectType_StaysOmitted() {
        string result = Pooshit.Json.Json.WriteString(new PlainGetOnlyIdData { Job = "Dev" });
        Assert.That(result, Does.Not.Contain("\"Id\""));
    }

    [Test, Parallelizable]
    [Description("Async counterpart of S3 on the plain reflection model path.")]
    public async Task WriteAsync_GetOnlyPropertyWithoutReflectType_StaysOmitted() {
        string result = await Pooshit.Json.Json.WriteStringAsync(new PlainGetOnlyIdData { Job = "Dev" });
        Assert.That(result, Does.Not.Contain("\"Id\""));
    }

    [Test, Parallelizable]
    [Description("S2/S4: a get-only property with [JsonWrite] emits on the plain reflection model path, agreeing with the source-generated path.")]
    public void Write_ComputedGetOnlyPropertyWithoutReflectType_EmitsValue() {
        string result = Pooshit.Json.Json.WriteString(new PlainComputedIdData { Job = "Dev" });
        Assert.That(result, Does.Contain("\"Id\":918273645"));
    }

    [Test, Parallelizable]
    [Description("Async counterpart of the S2/S4 reflection-path agreement guard.")]
    public async Task WriteAsync_ComputedGetOnlyPropertyWithoutReflectType_EmitsValue() {
        string result = await Pooshit.Json.Json.WriteStringAsync(new PlainComputedIdData { Job = "Dev" });
        Assert.That(result, Does.Contain("\"Id\":918273645"));
    }

    [Test, Parallelizable]
    [Description("D6 remedy is real: a private-set property with [JsonWrite] emits on the source-generated model path.")]
    public void Write_PrivateSetPropertyWithJsonWriteAndReflectType_Emits() {
        string result = Pooshit.Json.Json.WriteString(new JsonWritePrivateSetIdData(918273645L) { Job = "Dev" });
        Assert.That(result, Does.Contain("\"Id\":918273645"));
    }

    [Test, Parallelizable]
    [Description("Async counterpart of the D6 remedy guard on the source-generated model path.")]
    public async Task WriteAsync_PrivateSetPropertyWithJsonWriteAndReflectType_Emits() {
        string result = await Pooshit.Json.Json.WriteStringAsync(new JsonWritePrivateSetIdData(918273645L) { Job = "Dev" });
        Assert.That(result, Does.Contain("\"Id\":918273645"));
    }

    [Test, Parallelizable]
    [Description("D6 remedy is real: a private-set property with [JsonWrite] emits on the plain reflection model path.")]
    public void Write_PrivateSetPropertyWithJsonWriteWithoutReflectType_Emits() {
        string result = Pooshit.Json.Json.WriteString(new PlainJsonWritePrivateSetIdData(918273645L) { Job = "Dev" });
        Assert.That(result, Does.Contain("\"Id\":918273645"));
    }

    [Test, Parallelizable]
    [Description("Async counterpart of the D6 remedy guard on the plain reflection model path.")]
    public async Task WriteAsync_PrivateSetPropertyWithJsonWriteWithoutReflectType_Emits() {
        string result = await Pooshit.Json.Json.WriteStringAsync(new PlainJsonWritePrivateSetIdData(918273645L) { Job = "Dev" });
        Assert.That(result, Does.Contain("\"Id\":918273645"));
    }

    [Test, Parallelizable]
    [Description("Design §6.2 clause 2: [IgnoreDataMember] beats [JsonWrite] - the more restrictive attribute wins.")]
    public void Write_JsonWriteWithIgnoreDataMember_StaysOmitted() {
        string result = Pooshit.Json.Json.WriteString(new JsonWriteIgnoredIdData { Job = "Dev" });
        Assert.That(result, Does.Not.Contain("\"Id\""));
    }

    [Test, Parallelizable]
    [Description("Design §6.2 clause 1: [JsonWrite] on a set-only property has no effect and does not throw - readable stays an unconditional precondition.")]
    public void Write_JsonWriteOnSetOnlyProperty_StaysOmittedNoThrow() {
        string result = Pooshit.Json.Json.WriteString(new JsonWriteSetOnlyIdData { Job = "Dev", Id = 918273645L });
        Assert.That(result, Does.Not.Contain("\"Id\""));
    }

    [Test, Parallelizable]
    [Description("Design §6.2/§12.1: [JsonWrite] and [DataMember(Name=...)] are orthogonal and composable - nomination from one, key name from the other.")]
    public void Write_JsonWriteWithDataMemberName_EmitsUnderDataMemberName() {
        string result = Pooshit.Json.Json.WriteString(new JsonWriteDataMemberIdData { Job = "Dev" });
        Assert.That(result, Does.Contain("\"customId\":918273645"));
        Assert.That(result, Does.Not.Contain("\"Id\""));
    }

    [Test, Parallelizable]
    [Description("D3: [JsonWrite] applies only to the declaration it is written on (Inherited = false) - an override that does not re-declare it stays omitted on the source-generated model path.")]
    public void Write_JsonWriteOnOverriddenBasePropertyWithReflectType_StaysOmitted() {
        string result = Pooshit.Json.Json.WriteString(new JsonWriteOverrideDerivedData { Job = "Dev" });
        Assert.That(result, Does.Not.Contain("\"Id\""));
    }

    [Test, Parallelizable]
    [Description("D3: [JsonWrite] applies only to the declaration it is written on (Inherited = false) - an override that does not re-declare it stays omitted on the plain reflection model path too, so both model paths agree.")]
    public void Write_JsonWriteOnOverriddenBasePropertyWithoutReflectType_StaysOmitted() {
        string result = Pooshit.Json.Json.WriteString(new PlainJsonWriteOverrideDerivedData { Job = "Dev" });
        Assert.That(result, Does.Not.Contain("\"Id\""));
    }

    static Exception ThrownWithTargetSiteAndInnerException() {
        try {
            try {
                throw new InvalidOperationException("inner");
            }
            catch (Exception inner) {
                throw new Exception("boom", inner);
            }
        }
        catch (Exception thrown) {
            return thrown;
        }
    }

    [Test, Parallelizable]
    [Description("S1 regression guard (DiVoid #8522): serializing a thrown System.Exception (TargetSite and InnerException populated, matching what ErrorHandlerMiddleware actually catches) must terminate with finite output. TargetSite.DeclaringType is reference-equal to its own Type.UnderlyingSystemType, so an unbounded walk would not terminate; all these members are get-only and cannot carry [JsonWrite].")]
    public void Write_Exception_TerminatesWithFiniteOutputExcludingDangerousMembers() {
        string result = Pooshit.Json.Json.WriteString(ThrownWithTargetSiteAndInnerException());
        Assert.That(result, Does.Not.Contain("\"TargetSite\""));
        Assert.That(result, Does.Not.Contain("\"Data\""));
        Assert.That(result, Does.Not.Contain("\"InnerException\""));
        Assert.That(result, Does.Not.Contain("\"StackTrace\""));
        Dictionary<string, object> parsed = Pooshit.Json.Json.Read<Dictionary<string, object>>(result);
        Assert.NotNull(parsed);
    }

    [Test, Parallelizable]
    [Description("Async counterpart of the S1 Exception regression guard (DiVoid #8522).")]
    public async Task WriteAsync_Exception_TerminatesWithFiniteOutputExcludingDangerousMembers() {
        string result = await Pooshit.Json.Json.WriteStringAsync(ThrownWithTargetSiteAndInnerException());
        Assert.That(result, Does.Not.Contain("\"TargetSite\""));
        Assert.That(result, Does.Not.Contain("\"Data\""));
        Assert.That(result, Does.Not.Contain("\"InnerException\""));
        Assert.That(result, Does.Not.Contain("\"StackTrace\""));
        Dictionary<string, object> parsed = Pooshit.Json.Json.Read<Dictionary<string, object>>(result);
        Assert.NotNull(parsed);
    }

    [Test, Parallelizable]
    [Description("S1 regression guard (DiVoid #8522): serializing a System.Type must terminate with finite output. Type.UnderlyingSystemType is self-referential (returns 'this') and is get-only, so the walk must never start.")]
    public void Write_Type_TerminatesWithFiniteOutputExcludingDangerousMembers() {
        string result = Pooshit.Json.Json.WriteString(typeof(string));
        Assert.That(result, Does.Not.Contain("\"UnderlyingSystemType\""));
        Assert.That(result, Does.Not.Contain("\"DeclaringType\""));
        Assert.That(result, Does.Not.Contain("\"BaseType\""));
        Dictionary<string, object> parsed = Pooshit.Json.Json.Read<Dictionary<string, object>>(result);
        Assert.NotNull(parsed);
    }
}