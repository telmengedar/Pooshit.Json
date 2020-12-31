using System;
using System.IO;
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

        [Test, Parallelizable]
        public void ReadEmptyObject() {
            TestData testdata = NightlyCode.Json.Json.Read<TestData>("{}");
            Assert.NotNull(testdata);
        }

        [Test, Parallelizable]
        public void ReadGuid() {
            Guid guid = NightlyCode.Json.Json.Read<Guid>(NightlyCode.Json.Json.WriteString(Guid.Empty));
            Assert.AreEqual(Guid.Empty, guid);
        }

        [Test, Parallelizable]
        public void ReadGuidProperty() {
            TestData testdata = NightlyCode.Json.Json.Read<TestData>("{\"guid\":\"00000000-0000-0000-0000-000000000000\"}");
            Assert.NotNull(testdata);
            Assert.AreEqual(Guid.Empty, testdata.Guid);
        }
    }
}