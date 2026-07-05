using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Json.Tests.Data;
using NUnit.Framework;
using Pooshit.Json.Writer;

namespace Json.Tests {
    
    [TestFixture, Parallelizable]
    public class JsonStreamWriterTests {

        [Test, Parallelizable]
        public void WriteObject() {
            MemoryStream buffer = new();
            using (JsonStreamWriter writer = new(buffer)) {
                writer.BeginObject();
                writer.WriteProperty("test", new[] { 1, 2, 3, 4, 5 });
                writer.WriteProperty("next", 5.0);
                writer.EndObject();
            }

            string data = Encoding.UTF8.GetString(buffer.ToArray());
            Console.WriteLine(data);
            object result = Pooshit.Json.Json.Read(data);
        }

        [Test, Parallelizable]
        public void WriteObjectComplex() {
            MemoryStream buffer = new();
            using (JsonStreamWriter writer = new(buffer)) {
                writer.BeginObject();
                writer.WriteKey("test");
                writer.BeginArray();
                writer.WriteValue(1);
                writer.WriteValue(2);
                writer.WriteValue(3);
                writer.EndArray();
                writer.WriteProperty("next", 5.0);
                writer.EndObject();
            }

            string data = Encoding.UTF8.GetString(buffer.ToArray());
            Console.WriteLine(data);
            object result = Pooshit.Json.Json.Read(data);
        }

        [Test, Parallelizable]
        public async Task WriteObjectAsync() {
            MemoryStream buffer = new();
            await using (JsonStreamWriter writer = new(buffer)) {
                await writer.BeginObjectAsync();
                await writer.WritePropertyAsync("test", new[] { 1, 2, 3, 4, 5 });
                await writer.WritePropertyAsync("next", 5.0);
                await writer.EndObjectAsync();
            }

            object result = Pooshit.Json.Json.Read(Encoding.UTF8.GetString(buffer.ToArray()));
        }

        [Test, Parallelizable]
        public async Task WriteObjectComplexAsync() {
            MemoryStream buffer = new();
            await using (JsonStreamWriter writer = new(buffer)) {
                await writer.BeginObjectAsync();
                await writer.WriteKeyAsync("test");
                await writer.BeginArrayAsync();
                await writer.WriteValueAsync(1);
                await writer.WriteValueAsync(2);
                await writer.WriteValueAsync(3);
                await writer.EndArrayAsync();
                await writer.WritePropertyAsync("next", 5.0);
                await writer.EndObjectAsync();
            }

            string data = Encoding.UTF8.GetString(buffer.ToArray());
            Console.WriteLine(data);
            object result = Pooshit.Json.Json.Read(data);
        }

        [Test, Parallelizable]
        public async Task WriteObjectArrayUsingValueWrite() {
            MemoryStream buffer = new();
            await using (JsonStreamWriter writer = new(buffer)) {
                await writer.BeginArrayAsync();
                for (int i = 0; i < 5; ++i)
                    await writer.WriteValueAsync(new SnakeData {
                        OverTheTop = i
                    });
            }

            string data = Encoding.UTF8.GetString(buffer.ToArray());
            Console.WriteLine(data);
            object result = Pooshit.Json.Json.Read(data);
        }

        [Test, Parallelizable]
        public void WriteValueDictNullPreservedSync() {
            MemoryStream buffer = new();
            using (JsonStreamWriter writer = new(buffer)) {
                writer.WriteValue(new Dictionary<string, object> { ["a"] = "v", ["b"] = null });
            }
            string data = Encoding.UTF8.GetString(buffer.ToArray());
            Assert.That(data, Is.EqualTo("{\"a\":\"v\",\"b\":null}"));
        }

        [Test, Parallelizable]
        public async Task WriteValueDictNullPreservedAsync() {
            MemoryStream buffer = new();
            await using (JsonStreamWriter writer = new(buffer)) {
                await writer.WriteValueAsync(new Dictionary<string, object> { ["a"] = "v", ["b"] = null });
            }
            string data = Encoding.UTF8.GetString(buffer.ToArray());
            Assert.That(data, Is.EqualTo("{\"a\":\"v\",\"b\":null}"));
        }

        [Test, Parallelizable]
        public void WriteValueNestedDictNullPreservedSync() {
            MemoryStream buffer = new();
            using (JsonStreamWriter writer = new(buffer)) {
                writer.WriteValue(new Dictionary<string, object> {
                    ["outer"] = new Dictionary<string, object> { ["inner"] = null }
                });
            }
            string data = Encoding.UTF8.GetString(buffer.ToArray());
            Assert.That(data, Is.EqualTo("{\"outer\":{\"inner\":null}}"));
        }

        [Test, Parallelizable]
        public void WriteKeyWithQuoteEscapedSync() {
            MemoryStream buffer = new();
            using (JsonStreamWriter writer = new(buffer)) {
                writer.BeginObject();
                writer.WriteProperty("ke\"y", "val");
                writer.EndObject();
            }
            string data = Encoding.UTF8.GetString(buffer.ToArray());
            Assert.That(data, Is.EqualTo("{\"ke\\\"y\":\"val\"}"));
        }

        [Test, Parallelizable]
        public async Task WriteKeyWithQuoteEscapedAsync() {
            MemoryStream buffer = new();
            await using (JsonStreamWriter writer = new(buffer)) {
                await writer.BeginObjectAsync();
                await writer.WritePropertyAsync("ke\"y", "val");
                await writer.EndObjectAsync();
            }
            string data = Encoding.UTF8.GetString(buffer.ToArray());
            Assert.That(data, Is.EqualTo("{\"ke\\\"y\":\"val\"}"));
        }

        [Test, Parallelizable]
        public void WriteKeyWithBackslashEscapedSync() {
            MemoryStream buffer = new();
            using (JsonStreamWriter writer = new(buffer)) {
                writer.BeginObject();
                writer.WriteProperty("ke\\y", "val");
                writer.EndObject();
            }
            string data = Encoding.UTF8.GetString(buffer.ToArray());
            Assert.That(data, Is.EqualTo("{\"ke\\\\y\":\"val\"}"));
        }

        [Test, Parallelizable]
        public async Task WriteKeyWithBackslashEscapedAsync() {
            MemoryStream buffer = new();
            await using (JsonStreamWriter writer = new(buffer)) {
                await writer.BeginObjectAsync();
                await writer.WritePropertyAsync("ke\\y", "val");
                await writer.EndObjectAsync();
            }
            string data = Encoding.UTF8.GetString(buffer.ToArray());
            Assert.That(data, Is.EqualTo("{\"ke\\\\y\":\"val\"}"));
        }

        [Test, Parallelizable]
        public void WriteKeyWithNewlineEscapedSync() {
            MemoryStream buffer = new();
            using (JsonStreamWriter writer = new(buffer)) {
                writer.BeginObject();
                writer.WriteProperty("ke\ny", "val");
                writer.EndObject();
            }
            string data = Encoding.UTF8.GetString(buffer.ToArray());
            Assert.That(data, Is.EqualTo("{\"ke\\ny\":\"val\"}"));
        }

        [Test, Parallelizable]
        public async Task WriteKeyWithNewlineEscapedAsync() {
            MemoryStream buffer = new();
            await using (JsonStreamWriter writer = new(buffer)) {
                await writer.BeginObjectAsync();
                await writer.WritePropertyAsync("ke\ny", "val");
                await writer.EndObjectAsync();
            }
            string data = Encoding.UTF8.GetString(buffer.ToArray());
            Assert.That(data, Is.EqualTo("{\"ke\\ny\":\"val\"}"));
        }

        [Test, Parallelizable]
        public void WriteDictValueKeyWithQuoteEscapedSync() {
            MemoryStream buffer = new();
            using (JsonStreamWriter writer = new(buffer)) {
                writer.WriteValue(new Dictionary<string, object> { ["ke\"y"] = "val" });
            }
            string data = Encoding.UTF8.GetString(buffer.ToArray());
            Assert.That(data, Is.EqualTo("{\"ke\\\"y\":\"val\"}"));
        }

        [Test, Parallelizable]
        public async Task WriteDictValueKeyWithQuoteEscapedAsync() {
            MemoryStream buffer = new();
            await using (JsonStreamWriter writer = new(buffer)) {
                await writer.WriteValueAsync(new Dictionary<string, object> { ["ke\"y"] = "val" });
            }
            string data = Encoding.UTF8.GetString(buffer.ToArray());
            Assert.That(data, Is.EqualTo("{\"ke\\\"y\":\"val\"}"));
        }

        [Test, Parallelizable]
        public void StreamWriterIPAddressLoopbackSync() {
            MemoryStream buffer = new();
            using (JsonStreamWriter writer = new(buffer)) {
                writer.WriteValue(IPAddress.Loopback);
            }
            string data = Encoding.UTF8.GetString(buffer.ToArray());
            Assert.That(data, Is.EqualTo("\"127.0.0.1\""));
        }

        [Test, Parallelizable]
        public async Task StreamWriterIPAddressLoopbackAsync() {
            MemoryStream buffer = new();
            await using (JsonStreamWriter writer = new(buffer)) {
                await writer.WriteValueAsync(IPAddress.Loopback);
            }
            string data = Encoding.UTF8.GetString(buffer.ToArray());
            Assert.That(data, Is.EqualTo("\"127.0.0.1\""));
        }
    }
}