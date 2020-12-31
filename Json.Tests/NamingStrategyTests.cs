using Json.Tests.Data;
using NightlyCode.Json;
using NUnit.Framework;

namespace Json.Tests {
    
    [TestFixture]
    public class NamingStrategyTests {

        [Test]
        public void WriteCamelCase() {
            string result = NightlyCode.Json.Json.WriteString(new PropertyData {
                ID = 9,
                MagicCamel = 32
            }, new JsonOptions {
                ExcludeNullProperties = true,
                NamingStrategy = NamingStrategies.CamelCase
            });
            Assert.That(result.Contains("\"id\":9"));
            Assert.That(result.Contains("\"magicCamel\":32"));
        }
    }
}