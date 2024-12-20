﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Json.Tests.Data;
using NUnit.Framework;
using Pooshit.Json;
using Pooshit.Json.Tokens;

namespace Json.Tests;

[TestFixture, Parallelizable]
public class JPathTests {

    [Test, Parallelizable]
    public void SelectProperty() {
        object result = Pooshit.Json.Json.Read("{\"id\":97}");

        long value = JPath.Select<long>(result, "id");
        Assert.AreEqual(97, value);
    }

    [Test, Parallelizable]
    public void SelectPropertyIgnoreCase() {
        object result = Pooshit.Json.Json.Read("{\"id\":97}");

        long value = JPath.Select<long>(result, "Id", true);
        Assert.AreEqual(97, value);
    }

    [Test, Parallelizable]
    public void SelectPropertyFromObject() {
        PropertyData data = new() {
            ID = 2,
            MagicCamel = 6
        };

        long value = JPath.Select<long>(data, "MagicCamel");
        Assert.AreEqual(6, value);
    }

    [Test, Parallelizable]
    public void SelectPropertyFromObjectIgnoreCase() {
        PropertyData data = new() {
            ID = 2,
            MagicCamel = 6
        };

        long value = JPath.Select<long>(data, "magiccamel", true);
        Assert.AreEqual(6, value);
    }

    [Test, Parallelizable]
    public void SelectPropertyFromArray() {
        object result = Pooshit.Json.Json.Read("[{\"id\":97}, {\"id\":92}, {\"id\":90}]");

        IEnumerable values = JPath.Select<IEnumerable>(result, "id");
        Assert.That(values.Cast<long>().SequenceEqual([97L,92L,90L]));
    }

    [Test, Parallelizable]
    public void SelectPropertyFromArrayIgnoreCase() {
        object result = Pooshit.Json.Json.Read("[{\"id\":97}, {\"id\":92}, {\"id\":90}]");

        IEnumerable values = JPath.Select<IEnumerable>(result, "Id", true);
        Assert.That(values.Cast<long>().SequenceEqual([97L,92L,90L]));
    }

    [Test, Parallelizable]
    public void SelectSubPath() {
        Dictionary<string, object> dictionary = new() {
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
        Assert.That(((IEnumerable) array).Cast<object>().SequenceEqual(["larry", "garry", "harvey", "manny", "ann", "susan", "peter"]));
    }

    [Test, Parallelizable]
    public void ParseValidPath() {
        JPathToken[] tokens = JPath.Parse("configuration/values[82]/rhs").ToArray();
        Assert.AreEqual(4, tokens.Length);
        Assert.AreEqual("configuration", tokens[0].Property);
        Assert.AreEqual("values", tokens[1].Property);
        Assert.AreEqual(82, tokens[2].Index);
        Assert.AreEqual("rhs", tokens[3].Property);
    }

    [Test, Parallelizable]
    public void ParseSnakePath() {
        JPathToken[] tokens = JPath.Parse("token/access_token").ToArray();
        Assert.AreEqual(2, tokens.Length);
        Assert.AreEqual("access_token", tokens[1].Property);
    }
        
    [Test, Parallelizable]
    public void ExistsValid() {
        Dictionary<string, object> data = new() {
            ["configuration"] = new Dictionary<string, object> {
                ["values"] = new List<object> {
                    new Dictionary<string, object> {
                        ["lhs"] = 7,
                        ["rhs"] = 9
                    }
                }
            }
        };

        Assert.True(JPath.Exists(data, "configuration/values[0]/rhs"));
    }

    [Parallelizable]
    [TestCase("configuration/values[0]/bollocks")]
    [TestCase("configuration/values[82]/rhs")]
    [TestCase("configuration/bollocks[0]/rhs")]
    public void ExistsInvalid(string path) {
        Dictionary<string, object> data = new() {
            ["configuration"] = new Dictionary<string, object> {
                ["values"] = new List<object> {
                    new Dictionary<string, object> {
                        ["lhs"] = 7,
                        ["rhs"] = 9
                    }
                }
            }
        };

        Assert.False(JPath.Exists(data, path));
    }

    [Test, Parallelizable]
    public void SelectValue() {
        Dictionary<string, object> data = new() {
            ["configuration"] = new Dictionary<string, object> {
                ["values"] = new List<object> {
                    new Dictionary<string, object> {
                        ["lhs"] = 7,
                        ["rhs"] = 9
                    }
                }
            }
        };
            
        Assert.AreEqual(9, JPath.Select(data, "configuration/values[0]/rhs"));
    }

    [Test, Parallelizable]
    public void SelectValueIgnoreCase() {
        Dictionary<string, object> data = new() {
            ["configuration"] = new Dictionary<string, object> {
                ["values"] = new List<object> {
                    new Dictionary<string, object> {
                        ["lhs"] = 7,
                        ["rhs"] = 9
                    }
                }
            }
        };
            
        Assert.AreEqual(9, JPath.Select(data, "configuration/values[0]/Rhs", true));
    }

    [Test, Parallelizable]
    public void SetValue() {
        Dictionary<string, object> data = new();
        JPath.Set(data, "configuration/values[0]/rhs", 9);
        Assert.AreEqual(9, JPath.Select(data, "configuration/values[0]/rhs"));
    }
        
    [Test, Parallelizable]
    public void SetMultipleValues() {
        Dictionary<string, object> data = new();
        JPath.Set(data, "configuration/values[0]/lhs", 7);
        JPath.Set(data, "configuration/values[0]/rhs", 9);
        Assert.AreEqual(7, JPath.Select(data, "configuration/values[0]/lhs"));
        Assert.AreEqual(9, JPath.Select(data, "configuration/values[0]/rhs"));
    }

    [Test, Parallelizable]
    public void AccessHostByIndex() {
        int[] numbers = [1, 2, 3, 4, 5];
        Assert.AreEqual(3, JPath.Select<int>(numbers, "[2]"));
    }
    
    [Test, Parallelizable]
    public void SetPropertyIgnoreCase() {
        PropertyData data = new();
        
        JPath.Set(data, "magiccamel", 7, true);
        Assert.AreEqual(7, data.MagicCamel);
    }

}