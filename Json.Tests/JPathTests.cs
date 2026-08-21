using System;
using System.Collections;
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

    [Test, Parallelizable]
    public void SetDictionaryIgnoreCase_ExistingKeyDifferentCase_WritesValue() {
        Dictionary<string, object> data = new() {
            ["name"] = "old"
        };

        JPath.Set(data, "Name", "new-value", true);

        Assert.That(data["name"], Is.EqualTo("new-value"));
    }

    [Test, Parallelizable]
    public void SetDictionaryIgnoreCase_NewKey_WritesValue() {
        Dictionary<string, object> data = new();

        JPath.Set(data, "Name", "new-value", true);

        Assert.That(data["Name"], Is.EqualTo("new-value"));
    }

    [Test, Parallelizable]
    public void SetDictionaryCaseSensitive_WritesValueLiteral() {
        Dictionary<string, object> data = new() {
            ["name"] = "old"
        };

        JPath.Set(data, "name", "new-value");

        Assert.That(data["name"], Is.EqualTo("new-value"));
    }

    [Test, Parallelizable]
    public void SetDictionaryIgnoreCase_ExistingIntermediateKey_PreservesSiblingData() {
        Dictionary<string, object> data = new() {
            ["config"] = new Dictionary<string, object> {
                ["existing"] = "keep-me"
            }
        };

        JPath.Set(data, "Config/added", "val", true);

        Dictionary<string, object> config = (Dictionary<string, object>) data["config"];
        Assert.That(config["existing"], Is.EqualTo("keep-me"));
        Assert.That(config["added"], Is.EqualTo("val"));
    }

    [Test, Parallelizable]
    public void SetDictionaryIgnoreCase_MissingIntermediateKey_CreatesContainer() {
        Dictionary<string, object> data = new();

        JPath.Set(data, "Config/added", "val", true);

        Dictionary<string, object> config = (Dictionary<string, object>) data["Config"];
        Assert.That(config["added"], Is.EqualTo("val"));
    }

    [Test, Parallelizable]
    public void SetArrayIndex_InRange_WritesWithoutResize() {
        object[] data = ["a", "b", "c"];

        JPath.Set(data, "[1]", "z");

        Assert.That(data[1], Is.EqualTo("z"));
    }

    [Test, Parallelizable]
    public void SetArrayIndex_LeafGrowth_PropagatesResizedArrayToParent() {
        Dictionary<string, object> data = new() {
            ["items"] = new object[] { "a", "b" }
        };

        JPath.Set(data, "items[4]", "z");

        object[] items = (object[]) data["items"];
        Assert.That(items.Length, Is.EqualTo(5));
        Assert.That(items[4], Is.EqualTo("z"));
    }

    [Test, Parallelizable]
    public void SetNestedPath_IntermediateArrayGrowth_PropagatesResizedArrayToParent() {
        Dictionary<string, object> data = new() {
            ["items"] = new object[] { null, null }
        };

        JPath.Set(data, "items[3]/name", "bob");

        object[] items = (object[]) data["items"];
        Assert.That(items.Length, Is.EqualTo(4));
        Dictionary<string, object> created = (Dictionary<string, object>) items[3];
        Assert.That(created["name"], Is.EqualTo("bob"));
    }

    [Test, Parallelizable]
    public void SetNestedPath_ExistingNullArrayElementInRange_CreatesContainer() {
        Dictionary<string, object> data = new() {
            ["items"] = new object[] { null, null, null }
        };

        JPath.Set(data, "items[1]/name", "carl");

        object[] items = (object[]) data["items"];
        Dictionary<string, object> created = (Dictionary<string, object>) items[1];
        Assert.That(created["name"], Is.EqualTo("carl"));
    }

    [Test, Parallelizable]
    [Description("The array being grown has no parent reference to write the resized copy back into, so growth must fail loudly instead of silently discarding the write.")]
    public void SetArrayIndex_GrowRootArray_ThrowsInvalidOperation() {
        object[] data = ["a", "b"];

        Assert.Throws<InvalidOperationException>(() => JPath.Set(data, "[5]", "z"));
    }

    [Test, Parallelizable]
    public void SetListIndex_GrowsListInPlaceWithoutParentPropagation() {
        Dictionary<string, object> data = new() {
            ["items"] = new List<object> { "a", "b" }
        };

        JPath.Set(data, "items[4]", "z");

        List<object> items = (List<object>) data["items"];
        Assert.That(items.Count, Is.EqualTo(5));
        Assert.That(items[4], Is.EqualTo("z"));
    }

    [Test, Parallelizable]
    public void SetNestedPocoProperty_GrowsSettablePropertyArrayAndPropagates() {
        NestedArrayPropertyData data = new() {
            Items = ["a", "b"]
        };

        JPath.Set(data, "Items[4]", "z");

        Assert.That(data.Items.Length, Is.EqualTo(5));
        Assert.That(data.Items[4], Is.EqualTo("z"));
    }

    [Test, Parallelizable]
    [Description("A get-only property has no way to receive a resized array, so growth must fail the same way root-level growth does rather than leaking a reflection ArgumentException.")]
    public void SetGetOnlyPocoProperty_GrowArray_ThrowsInvalidOperation() {
        GetOnlyArrayPropertyData data = new();

        Assert.Throws<InvalidOperationException>(() => JPath.Set(data, "Items[4]", "z"));
    }

    [Test, Parallelizable]
    public void SetArrayInArray_LeafGrowth_PropagatesThroughIndexWriteBack() {
        object[] data = [new object[] { "a", "b" }];

        JPath.Set(data, "[0][3]", "z");

        object[] inner = (object[]) data[0];
        Assert.That(inner.Length, Is.EqualTo(4));
        Assert.That(inner[3], Is.EqualTo("z"));
    }

    [Test, Parallelizable]
    public void SetArrayInArray_IntermediateGrowth_PropagatesThroughIndexWriteBack() {
        object[] data = [new object[] { null, null }];

        JPath.Set(data, "[0][3]/name", "bob");

        object[] inner = (object[]) data[0];
        Assert.That(inner.Length, Is.EqualTo(4));
        Dictionary<string, object> created = (Dictionary<string, object>) inner[3];
        Assert.That(created["name"], Is.EqualTo("bob"));
    }

    [Test, Parallelizable]
    [Description("The intermediate array-growth site carries the same no-parent guard as the leaf site, so a root array must fail loudly there too instead of silently discarding the write.")]
    public void SetArrayIndexIntermediate_GrowRootArray_ThrowsInvalidOperation() {
        object[] data = ["a", "b"];

        Assert.Throws<InvalidOperationException>(() => JPath.Set(data, "[3]/name", "x"));
    }

}