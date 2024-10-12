using Json.Tests.Data;
using NUnit.Framework;
using Pooshit.Json;

namespace Json.Tests {
    
    [TestFixture]
    public class NamingStrategyTests {

        [Test]
        public void WriteCamelCase() {
            string result = Pooshit.Json.Json.WriteString(new PropertyData {
                                                                               ID = 9,
                                                                               MagicCamel = 32
                                                                           }, new() {
                                                                                        ExcludeNullProperties = true,
                                                                                        NamingStrategy = NamingStrategies.CamelCase
                                                                                    });
            Assert.That(result.Contains("\"id\":9"));
            Assert.That(result.Contains("\"magicCamel\":32"));
        }
    }
}