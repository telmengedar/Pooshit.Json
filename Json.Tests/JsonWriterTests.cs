using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Json.Tests.Data;
using NightlyCode.Json;
using NUnit.Framework;

namespace Json.Tests {
    
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
            string result = NightlyCode.Json.Json.WriteString(data);
            Assert.AreEqual(expected, result);
        }

        [Test, Parallelizable]
        public void WriteDateTime() {
            string result = NightlyCode.Json.Json.WriteString(new DateTime(2020, 12, 31));
            Assert.AreEqual("\"2020-12-31T00:00:00.0000000\"", result);
        }

        [Test, Parallelizable]
        public async Task WriteDateTimeAsync() {
            string result = await NightlyCode.Json.Json.WriteStringAsync(new DateTime(2020, 12, 31));
            Assert.AreEqual("\"2020-12-31T00:00:00.0000000\"", result);
        }

        [Test, Parallelizable]
        public void WriteTimeSpan() {
            string result = NightlyCode.Json.Json.WriteString(new TimeSpan(4, 34, 22));
            Assert.AreEqual("\"04:34:22\"", result);
        }

        [Test, Parallelizable]
        public async Task WriteTimeSpanAsync() {
            string result = await NightlyCode.Json.Json.WriteStringAsync(new TimeSpan(4, 34, 22));
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
            string result = await NightlyCode.Json.Json.WriteStringAsync(data);
            Assert.AreEqual(expected, result);
        }

        [Test, Parallelizable]
        public void WriteDecimal() {
            string result = NightlyCode.Json.Json.WriteString(13.44m);
            Assert.AreEqual("13.44", result);
        }

        [Test, Parallelizable]
        public async Task WriteDecimalAsync() {
            string result = await NightlyCode.Json.Json.WriteStringAsync(13.44m);
            Assert.AreEqual("13.44", result);
        }

        [Test, Parallelizable]
        public void WriteGuid() {
            string result = NightlyCode.Json.Json.WriteString(Guid.Empty);
            Assert.AreEqual($"\"{Guid.Empty}\"", result);
        }

        [Test, Parallelizable]
        public async Task WriteGuidAsync() {
            string result = await NightlyCode.Json.Json.WriteStringAsync(Guid.Empty);
            Assert.AreEqual($"\"{Guid.Empty}\"", result);
        }

        [Test, Parallelizable]
        public void WriteObject() {
            string result = NightlyCode.Json.Json.WriteString(new TestData {
                Decimal = 0.22m,
                Long = 92,
                String = "Hello",
                Array = new[] {1, 5, 4, 3, 3},
                ChildTestData = new TestData {
                    String = "bums\"bom"
                }
            });

            TestData testdata = NightlyCode.Json.Json.Read<TestData>(result);
            Assert.AreEqual(0.22m, testdata.Decimal);
            Assert.AreEqual(92, testdata.Long);
            Assert.AreEqual("Hello", testdata.String);
            Assert.That(new[] {1, 5, 4, 3, 3}.SequenceEqual(testdata.Array));
            Assert.NotNull(testdata.ChildTestData);
            Assert.AreEqual("bums\"bom", testdata.ChildTestData.String);
        }

        [Test, Parallelizable]
        public async Task WriteObjectAsync() {
            string result = await NightlyCode.Json.Json.WriteStringAsync(new TestData {
                Decimal = 0.22m,
                Long = 92,
                String = "Hello",
                Array = new[] {1, 5, 4, 3, 3},
                ChildTestData = new TestData {
                    String = "bums\"bom"
                }
            });

            TestData testdata = await NightlyCode.Json.Json.ReadAsync<TestData>(result);
            Assert.AreEqual(0.22m, testdata.Decimal);
            Assert.AreEqual(92, testdata.Long);
            Assert.AreEqual("Hello", testdata.String);
            Assert.That(new[] {1, 5, 4, 3, 3}.SequenceEqual(testdata.Array));
            Assert.NotNull(testdata.ChildTestData);
            Assert.AreEqual("bums\"bom", testdata.ChildTestData.String);
        }

        [Test, Parallelizable]
        public void WriteEnum() {
            string result = NightlyCode.Json.Json.WriteString(DayOfWeek.Tuesday);
            Assert.AreEqual("2", result);
        }

        [Test, Parallelizable]
        public async Task WriteEnumAsync() {
            string result = await NightlyCode.Json.Json.WriteStringAsync(DayOfWeek.Tuesday);
            Assert.AreEqual("2", result);
        }

        [Test, Parallelizable]
        public void WriteEnumString() {
            string result = NightlyCode.Json.Json.WriteString(DayOfWeek.Tuesday, new JsonOptions {
                WriteEnumsAsStrings = true
            });
            Assert.AreEqual("\"Tuesday\"", result);
        }

        [Test, Parallelizable]
        public async Task WriteEnumStringAsync() {
            string result = await NightlyCode.Json.Json.WriteStringAsync(DayOfWeek.Tuesday, new JsonOptions {
                WriteEnumsAsStrings = true
            });
            Assert.AreEqual("\"Tuesday\"", result);
        }

        [Test, Parallelizable]
        public void WriteNullableNull() {
            int? data = null;
            // ReSharper disable once ExpressionIsAlwaysNull
            string result = NightlyCode.Json.Json.WriteString(data);
            Assert.AreEqual("null", result);
        }

        [Test, Parallelizable]
        public async Task WriteNullableNullAsync() {
            int? data = null;
            // ReSharper disable once ExpressionIsAlwaysNull
            string result = await NightlyCode.Json.Json.WriteStringAsync(data);
            Assert.AreEqual("null", result);
        }

        [Test, Parallelizable]
        public void WriteNullableValue() {
            int? data = 92;
            string result = NightlyCode.Json.Json.WriteString(data);
            Assert.AreEqual("92", result);
        }
        
        [Test, Parallelizable]
        public async Task WriteNullableValueAsync() {
            int? data = 92;
            string result = await NightlyCode.Json.Json.WriteStringAsync(data);
            Assert.AreEqual("92", result);
        }

        [Test, Parallelizable]
        public void WriteEscapedString() {
            string result = NightlyCode.Json.Json.WriteString("Hello\nNext Line\n\ttabbed content");
            Assert.AreEqual("\"Hello\\nNext Line\\n\\ttabbed content\"",result);
        }
        
        [Test, Parallelizable]
        public async Task WriteEscapedStringAsync() {
            string result = await NightlyCode.Json.Json.WriteStringAsync("Hello\nNext Line\n\ttabbed content");
            Assert.AreEqual("\"Hello\\nNext Line\\n\\ttabbed content\"",result);
        }

        [Test, Parallelizable]
        public void WriteDataWithIndexer() {
            string result = NightlyCode.Json.Json.WriteString(new DataWithIndexer());
        }

        [Test, Parallelizable]
        public void WriteList() {
            string result = NightlyCode.Json.Json.WriteString(new List<object>() {1, 2, 3, 4, 5});
            Assert.AreEqual("[1,2,3,4,5]", result);
        }
        
        [Test, Parallelizable]
        public async Task WriteListAsync() {
            string result = await NightlyCode.Json.Json.WriteStringAsync(new List<object>() {1, 2, 3, 4, 5});
            Assert.AreEqual("[1,2,3,4,5]", result);
        }
        
        [Test, Parallelizable]
        public void IgnoreAttribute() {
            string result = NightlyCode.Json.Json.WriteString(new DataWithIgnoredProperties());
            Assert.AreEqual("{\"Visible\":0}", result);
        }
        
        [Test, Parallelizable]
        public async Task IgnoreAttributeAsync() {
            string result = await NightlyCode.Json.Json.WriteStringAsync(new DataWithIgnoredProperties());
            Assert.AreEqual("{\"Visible\":0}", result);
        }

    }
}