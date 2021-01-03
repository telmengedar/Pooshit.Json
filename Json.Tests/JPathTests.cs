using System.Collections;
using System.Collections.Generic;
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

        [Test, Parallelizable]
        public void SelectSubPath() {
            Dictionary<string, object> dictionary = new Dictionary<string, object>() {
                ["array"] = new object[] {
                    new Dictionary<string, object> {
                        ["persons"] = new object[] {
                            new Dictionary<string, object> {
                                ["name"] = "larry",
                                ["age"] = 7
                            },
                            new Dictionary<string, object> {
                                ["name"] = "garry",
                                ["age"] = 9
                            },
                            new Dictionary<string, object> {
                                ["name"] = "harvey",
                                ["age"] = 10
                            },
                        }
                    },
                    new Dictionary<string, object> {
                        ["persons"] = new object[] {
                            new Dictionary<string, object> {
                                ["name"] = "manny",
                                ["age"] = 7
                            },
                            new Dictionary<string, object> {
                                ["name"] = "ann",
                                ["age"] = 9
                            },
                            new Dictionary<string, object> {
                                ["name"] = "susan",
                                ["age"] = 10
                            },
                        }
                    },
                    new Dictionary<string, object> {
                        ["persons"] = new object[] {
                            new Dictionary<string, object> {
                                ["name"] = "peter",
                                ["age"] = 7
                            }
                        }
                    }
                }
            };

            object array = JPath.Select(dictionary, "array/persons/name");
            Assert.NotNull(array);
            Assert.That(array is IEnumerable);
            Assert.That(((IEnumerable) array).Cast<object>().SequenceEqual(new[] {"larry", "garry", "harvey", "manny", "ann", "susan", "peter"}));
        }
    }
}