using System.Collections;
using System.Linq;
using NightlyCode.Json;
using NUnit.Framework;

namespace Json.Tests {
    
    [TestFixture, Parallelizable]
    public class JPathTests {

        [Test, Parallelizable]
        public void SelectProperty() {
            object result = NightlyCode.Json.Json.Read("{\"id\":97}");

            long value = JPath.Select<long>(result, "id");
            Assert.AreEqual(97, value);
        }
        
        [Test, Parallelizable]
        public void SelectPropertyFromArray() {
            object result = NightlyCode.Json.Json.Read("[{\"id\":97}, {\"id\":92}, {\"id\":90}]");

            IEnumerable values = JPath.Select<IEnumerable>(result, "id");
            Assert.That(values.Cast<long>().SequenceEqual(new[]{97L,92L,90L}));
        }

    }
}