using System;
using System.IO;
using System.Threading.Tasks;
using Json.Tests.Data;
using NUnit.Framework;

namespace Json.Tests {
    [TestFixture, Parallelizable]
    public class JsonReaderTests {

        [TestCase("Json.Tests.Data.map.json")]
        [Parallelizable]
        public void ReadValidJson(string resource) {
            using Stream datastream = typeof(JsonReaderTests).Assembly.GetManifestResourceStream(resource);
            object result = NightlyCode.Json.Json.Read(datastream);
            Assert.NotNull(result);
        }

        [TestCase("Json.Tests.Data.map.json")]
        [Parallelizable]
        public async Task ReadValidJsonAsync(string resource) {
            using Stream datastream = typeof(JsonReaderTests).Assembly.GetManifestResourceStream(resource);
            object result = await NightlyCode.Json.Json.ReadAsync(datastream);
            Assert.NotNull(result);
        }

        [Test, Parallelizable]
        public void ReadEmptyObject() {
            TestData testdata = NightlyCode.Json.Json.Read<TestData>("{}");
            Assert.NotNull(testdata);
        }

        [Test, Parallelizable]
        public async Task ReadEmptyObjectAsync() {
            TestData testdata = await NightlyCode.Json.Json.ReadAsync<TestData>("{}");
            Assert.NotNull(testdata);
        }

        [Test, Parallelizable]
        public void ReadGuid() {
            Guid guid = NightlyCode.Json.Json.Read<Guid>(NightlyCode.Json.Json.WriteString(Guid.Empty));
            Assert.AreEqual(Guid.Empty, guid);
        }

        [Test, Parallelizable]
        public async Task ReadGuidAsync() {
            Guid guid = await NightlyCode.Json.Json.ReadAsync<Guid>(NightlyCode.Json.Json.WriteString(Guid.Empty));
            Assert.AreEqual(Guid.Empty, guid);
        }

        [Test, Parallelizable]
        public void ReadGuidProperty() {
            TestData testdata = NightlyCode.Json.Json.Read<TestData>("{\"guid\":\"00000000-0000-0000-0000-000000000000\"}");
            Assert.NotNull(testdata);
            Assert.AreEqual(Guid.Empty, testdata.Guid);
        }
        
        [Test, Parallelizable]
        public async Task ReadGuidPropertyAsync() {
            TestData testdata = await NightlyCode.Json.Json.ReadAsync<TestData>("{\"guid\":\"00000000-0000-0000-0000-000000000000\"}");
            Assert.NotNull(testdata);
            Assert.AreEqual(Guid.Empty, testdata.Guid);
        }
    }
}