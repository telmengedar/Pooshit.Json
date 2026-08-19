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

        [Test, Parallelizable]
        public void WriteValue_ComputedGetOnlyProperty_EmitsValue() {
            MemoryStream buffer = new();
            using (JsonStreamWriter writer = new(buffer)) {
                writer.WriteValue(new ComputedIdData { Job = "Dev" });
            }
            string data = Encoding.UTF8.GetString(buffer.ToArray());
            Assert.That(data, Does.Contain("\"Id\":918273645"));
        }

        [Test, Parallelizable]
        public async Task WriteValueAsync_ComputedGetOnlyProperty_EmitsValue() {
            MemoryStream buffer = new();
            await using (JsonStreamWriter writer = new(buffer)) {
                await writer.WriteValueAsync(new ComputedIdData { Job = "Dev" });
            }
            string data = Encoding.UTF8.GetString(buffer.ToArray());
            Assert.That(data, Does.Contain("\"Id\":918273645"));
        }

        [Test, Parallelizable]
        public void WriteValue_SetOnlyProperty_StaysOmitted() {
            MemoryStream buffer = new();
            using (JsonStreamWriter writer = new(buffer)) {
                writer.WriteValue(new SetOnlyIdData { Job = "Dev", Id = 918273645L });
            }
            string data = Encoding.UTF8.GetString(buffer.ToArray());
            Assert.That(data, Does.Not.Contain("\"Id\""));
        }

        [Test, Parallelizable]
        [Description("S3: a get-only property without [JsonWrite] stays omitted on the raw-reflection stream writer path.")]
        public void WriteValue_GetOnlyProperty_StaysOmitted() {
            MemoryStream buffer = new();
            using (JsonStreamWriter writer = new(buffer)) {
                writer.WriteValue(new PlainGetOnlyIdData { Job = "Dev" });
            }
            string data = Encoding.UTF8.GetString(buffer.ToArray());
            Assert.That(data, Does.Not.Contain("\"Id\""));
        }

        [Test, Parallelizable]
        [Description("Async counterpart of S3 on the raw-reflection stream writer path.")]
        public async Task WriteValueAsync_GetOnlyProperty_StaysOmitted() {
            MemoryStream buffer = new();
            await using (JsonStreamWriter writer = new(buffer)) {
                await writer.WriteValueAsync(new PlainGetOnlyIdData { Job = "Dev" });
            }
            string data = Encoding.UTF8.GetString(buffer.ToArray());
            Assert.That(data, Does.Not.Contain("\"Id\""));
        }

        [Test, Parallelizable]
        [Description("D6 remedy (design §6.3): [JsonWrite] on a private-set property emits on the stream writer path.")]
        public void WriteValue_PrivateSetPropertyWithJsonWrite_Emits() {
            MemoryStream buffer = new();
            using (JsonStreamWriter writer = new(buffer)) {
                writer.WriteValue(new PlainJsonWritePrivateSetIdData(918273645L) { Job = "Dev" });
            }
            string data = Encoding.UTF8.GetString(buffer.ToArray());
            Assert.That(data, Does.Contain("\"Id\":918273645"));
        }

        [Test, Parallelizable]
        [Description("Async counterpart of the D6 remedy guard (design §6.3) on the stream writer path.")]
        public async Task WriteValueAsync_PrivateSetPropertyWithJsonWrite_Emits() {
            MemoryStream buffer = new();
            await using (JsonStreamWriter writer = new(buffer)) {
                await writer.WriteValueAsync(new PlainJsonWritePrivateSetIdData(918273645L) { Job = "Dev" });
            }
            string data = Encoding.UTF8.GetString(buffer.ToArray());
            Assert.That(data, Does.Contain("\"Id\":918273645"));
        }

        [Test, Parallelizable]
        [Description("Break 2 guard (design §17) on the stream writer path: a private-set property without [JsonWrite] stays omitted.")]
        public void WriteValue_PrivateSetPropertyWithoutJsonWrite_StaysOmitted() {
            MemoryStream buffer = new();
            using (JsonStreamWriter writer = new(buffer)) {
                writer.WriteValue(new PlainPrivateSetIdData(918273645L) { Job = "Dev" });
            }
            string data = Encoding.UTF8.GetString(buffer.ToArray());
            Assert.That(data, Does.Not.Contain("\"Id\""));
        }

        [Test, Parallelizable]
        [Description("Async counterpart of the Break 2 guard (design §17) on the stream writer path.")]
        public async Task WriteValueAsync_PrivateSetPropertyWithoutJsonWrite_StaysOmitted() {
            MemoryStream buffer = new();
            await using (JsonStreamWriter writer = new(buffer)) {
                await writer.WriteValueAsync(new PlainPrivateSetIdData(918273645L) { Job = "Dev" });
            }
            string data = Encoding.UTF8.GetString(buffer.ToArray());
            Assert.That(data, Does.Not.Contain("\"Id\""));
        }

        [Test, Parallelizable]
        [Description("Design §6.2 clause 2 on the stream writer path: [IgnoreDataMember] beats [JsonWrite].")]
        public void WriteValue_JsonWriteWithIgnoreDataMember_StaysOmitted() {
            MemoryStream buffer = new();
            using (JsonStreamWriter writer = new(buffer)) {
                writer.WriteValue(new JsonWriteIgnoredIdData { Job = "Dev" });
            }
            string data = Encoding.UTF8.GetString(buffer.ToArray());
            Assert.That(data, Does.Not.Contain("\"Id\""));
        }

        [Test, Parallelizable]
        [Description("Design §6.2 clause 2 on the stream writer path (async): [IgnoreDataMember] beats [JsonWrite].")]
        public async Task WriteValueAsync_JsonWriteWithIgnoreDataMember_StaysOmitted() {
            MemoryStream buffer = new();
            await using (JsonStreamWriter writer = new(buffer)) {
                await writer.WriteValueAsync(new JsonWriteIgnoredIdData { Job = "Dev" });
            }
            string data = Encoding.UTF8.GetString(buffer.ToArray());
            Assert.That(data, Does.Not.Contain("\"Id\""));
        }

        [Test, Parallelizable]
        [Description("Design §6.2 clause 1 on the stream writer path: [JsonWrite] on a set-only property has no effect and does not throw.")]
        public void WriteValue_JsonWriteOnSetOnlyProperty_StaysOmittedNoThrow() {
            MemoryStream buffer = new();
            using (JsonStreamWriter writer = new(buffer)) {
                writer.WriteValue(new JsonWriteSetOnlyIdData { Job = "Dev", Id = 918273645L });
            }
            string data = Encoding.UTF8.GetString(buffer.ToArray());
            Assert.That(data, Does.Not.Contain("\"Id\""));
        }

        [Test, Parallelizable]
        [Description("Design §6.2 clause 1 on the stream writer path (async): [JsonWrite] on a set-only property has no effect and does not throw.")]
        public async Task WriteValueAsync_JsonWriteOnSetOnlyProperty_StaysOmittedNoThrow() {
            MemoryStream buffer = new();
            await using (JsonStreamWriter writer = new(buffer)) {
                await writer.WriteValueAsync(new JsonWriteSetOnlyIdData { Job = "Dev", Id = 918273645L });
            }
            string data = Encoding.UTF8.GetString(buffer.ToArray());
            Assert.That(data, Does.Not.Contain("\"Id\""));
        }

        [Test, Parallelizable]
        [Description("(DiVoid #8466, design §7.3): the stream writer bypasses the model layer and ignores [DataMember(Name=...)], so a [JsonWrite] property emits under its raw property name here, not the DataMember name.")]
        public void WriteValue_JsonWriteWithDataMemberName_EmitsUnderRawPropertyName() {
            MemoryStream buffer = new();
            using (JsonStreamWriter writer = new(buffer)) {
                writer.WriteValue(new JsonWriteDataMemberIdData { Job = "Dev" });
            }
            string data = Encoding.UTF8.GetString(buffer.ToArray());
            Assert.That(data, Does.Contain("\"Id\":918273645"));
            Assert.That(data, Does.Not.Contain("\"customId\""));
        }

        [Test, Parallelizable]
        [Description("Async counterpart of the DataMember-composability guard (DiVoid #8466, design §7.3) on the stream writer path.")]
        public async Task WriteValueAsync_JsonWriteWithDataMemberName_EmitsUnderRawPropertyName() {
            MemoryStream buffer = new();
            await using (JsonStreamWriter writer = new(buffer)) {
                await writer.WriteValueAsync(new JsonWriteDataMemberIdData { Job = "Dev" });
            }
            string data = Encoding.UTF8.GetString(buffer.ToArray());
            Assert.That(data, Does.Contain("\"Id\":918273645"));
            Assert.That(data, Does.Not.Contain("\"customId\""));
        }

        [Test, Parallelizable]
        [Description("D3 on the stream writer path: [JsonWrite] applies only to the declaration it is written on (Inherited = false) - an override that does not re-declare it stays omitted.")]
        public void WriteValue_JsonWriteOnOverriddenBaseProperty_StaysOmitted() {
            MemoryStream buffer = new();
            using (JsonStreamWriter writer = new(buffer)) {
                writer.WriteValue(new PlainJsonWriteOverrideDerivedData { Job = "Dev" });
            }
            string data = Encoding.UTF8.GetString(buffer.ToArray());
            Assert.That(data, Does.Not.Contain("\"Id\""));
        }

        [Test, Parallelizable]
        [Description("D3 on the stream writer path (async): [JsonWrite] applies only to the declaration it is written on (Inherited = false).")]
        public async Task WriteValueAsync_JsonWriteOnOverriddenBaseProperty_StaysOmitted() {
            MemoryStream buffer = new();
            await using (JsonStreamWriter writer = new(buffer)) {
                await writer.WriteValueAsync(new PlainJsonWriteOverrideDerivedData { Job = "Dev" });
            }
            string data = Encoding.UTF8.GetString(buffer.ToArray());
            Assert.That(data, Does.Not.Contain("\"Id\""));
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
        [Description("S1 regression guard (DiVoid #8522) on the stream writer path: a thrown System.Exception must terminate with finite output.")]
        public void WriteValue_Exception_TerminatesWithFiniteOutputExcludingDangerousMembers() {
            MemoryStream buffer = new();
            using (JsonStreamWriter writer = new(buffer)) {
                writer.WriteValue(ThrownWithTargetSiteAndInnerException());
            }
            string data = Encoding.UTF8.GetString(buffer.ToArray());
            Assert.That(data, Does.Not.Contain("\"TargetSite\""));
            Assert.That(data, Does.Not.Contain("\"Data\""));
            Assert.That(data, Does.Not.Contain("\"InnerException\""));
            Assert.That(data, Does.Not.Contain("\"StackTrace\""));
            object parsed = Pooshit.Json.Json.Read(data);
            Assert.NotNull(parsed);
        }

        [Test, Parallelizable]
        [Description("Async counterpart of the S1 Exception regression guard (DiVoid #8522) on the stream writer path.")]
        public async Task WriteValueAsync_Exception_TerminatesWithFiniteOutputExcludingDangerousMembers() {
            MemoryStream buffer = new();
            await using (JsonStreamWriter writer = new(buffer)) {
                await writer.WriteValueAsync(ThrownWithTargetSiteAndInnerException());
            }
            string data = Encoding.UTF8.GetString(buffer.ToArray());
            Assert.That(data, Does.Not.Contain("\"TargetSite\""));
            Assert.That(data, Does.Not.Contain("\"Data\""));
            Assert.That(data, Does.Not.Contain("\"InnerException\""));
            Assert.That(data, Does.Not.Contain("\"StackTrace\""));
            object parsed = Pooshit.Json.Json.Read(data);
            Assert.NotNull(parsed);
        }
    }
}