using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Json.Tests.Data;
using NightlyCode.Json.Writer;
using NUnit.Framework;

namespace Json.Tests {
    
    [TestFixture, Parallelizable]
    public class JsonStreamWriterTests {

        [Test, Parallelizable]
        public void WriteObject() {
            MemoryStream buffer = new MemoryStream();
            using (JsonStreamWriter writer = new JsonStreamWriter(buffer)) {
                writer.BeginObject();
                writer.WriteProperty("test", new[] { 1, 2, 3, 4, 5 });
                writer.WriteProperty("next", 5.0);
                writer.EndObject();
            }

            string data = Encoding.UTF8.GetString(buffer.ToArray());
            Console.WriteLine(data);
            object result = NightlyCode.Json.Json.Read(data);
        }

        [Test, Parallelizable]
        public void WriteObjectComplex() {
            MemoryStream buffer = new MemoryStream();
            using (JsonStreamWriter writer = new JsonStreamWriter(buffer)) {
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
            object result = NightlyCode.Json.Json.Read(data);
        }

        [Test, Parallelizable]
        public async Task WriteObjectAsync() {
            MemoryStream buffer = new MemoryStream();
            using (JsonStreamWriter writer = new JsonStreamWriter(buffer)) {
                await writer.BeginObjectAsync();
                await writer.WritePropertyAsync("test", new[] { 1, 2, 3, 4, 5 });
                await writer.WritePropertyAsync("next", 5.0);
                await writer.EndObjectAsync();
            }

            object result = NightlyCode.Json.Json.Read(Encoding.UTF8.GetString(buffer.ToArray()));
        }

        [Test, Parallelizable]
        public async Task WriteObjectComplexAsync() {
            MemoryStream buffer = new MemoryStream();
            await using (JsonStreamWriter writer = new JsonStreamWriter(buffer)) {
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
            object result = NightlyCode.Json.Json.Read(data);
        }

        [Test, Parallelizable]
        public async Task WriteObjectArrayUsingValueWrite() {
            MemoryStream buffer = new MemoryStream();
            await using (JsonStreamWriter writer = new JsonStreamWriter(buffer)) {
                await writer.BeginArrayAsync();
                for (int i = 0; i < 5; ++i)
                    await writer.WriteValueAsync(new SnakeData {
                        OverTheTop = i
                    });
            }

            string data = Encoding.UTF8.GetString(buffer.ToArray());
            Console.WriteLine(data);
            object result = NightlyCode.Json.Json.Read(data);
        }
    }
}