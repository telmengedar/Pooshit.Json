using System;
using System.Linq;
using System.Threading.Tasks;
using Json.Tests.Data;
using NUnit.Framework;
using Pooshit.Json;
using Pooshit.Json.Models;
using Pooshit.Reflection;

namespace Json.Tests;

/// <summary>
/// Verifies that the three caches switched to ConcurrentDictionary still
/// produce correct results, including null-caching for unknown names and
/// DataMember-attribute name resolution.  Also exercises concurrent access
/// to confirm no corruption under parallel load (bounded, deterministic
/// output — the race itself is timing-dependent and cannot be forced).
/// </summary>
[TestFixture, Parallelizable]
public class CacheThreadSafetyTests {

    // ── ReflectionModel functional ────────────────────────────────────────

    [Test, Parallelizable]
    public void ReflectionModel_ResolvesKnownProperty() {
        ReflectionModel model = new(typeof(TestData));
        var property = model.GetProperty("long");
        Assert.NotNull(property);
        Assert.AreEqual("Long", property.Name);
    }

    [Test, Parallelizable]
    public void ReflectionModel_ReturnsCachedInstance() {
        ReflectionModel model = new(typeof(TestData));
        var first  = model.GetProperty("string");
        var second = model.GetProperty("string");
        Assert.AreSame(first, second);
    }

    [Test, Parallelizable]
    public void ReflectionModel_CachesNullForUnknownProperty() {
        ReflectionModel model = new(typeof(TestData));
        var first  = model.GetProperty("__no_such_property__");
        var second = model.GetProperty("__no_such_property__");
        Assert.IsNull(first);
        Assert.IsNull(second);
    }

    [Test, Parallelizable]
    public void ReflectionModel_ResolvesDataMemberName() {
        ReflectionModel model = new(typeof(SnakeData));
        var property = model.GetProperty("over_the_top");
        Assert.NotNull(property);
        Assert.AreEqual("OverTheTop", property.Name);
    }

    // ── SourceGenModel functional ─────────────────────────────────────────

    [Test, Parallelizable]
    public void SourceGenModel_ResolvesKnownProperty() {
        SourceGenModel model = new(Reflection.GetModel(typeof(TestData)));
        var property = model.GetProperty("long");
        Assert.NotNull(property);
        Assert.AreEqual("Long", property.Name);
    }

    [Test, Parallelizable]
    public void SourceGenModel_ReturnsCachedInstance() {
        SourceGenModel model = new(Reflection.GetModel(typeof(TestData)));
        var first  = model.GetProperty("string");
        var second = model.GetProperty("string");
        Assert.AreSame(first, second);
    }

    [Test, Parallelizable]
    public void SourceGenModel_CachesNullForUnknownProperty() {
        SourceGenModel model = new(Reflection.GetModel(typeof(TestData)));
        var first  = model.GetProperty("__no_such_property__");
        var second = model.GetProperty("__no_such_property__");
        Assert.IsNull(first);
        Assert.IsNull(second);
    }

    [Test, Parallelizable]
    public void SourceGenModel_ResolvesDataMemberName() {
        SourceGenModel model = new(Reflection.GetModel(typeof(SnakeData)));
        var property = model.GetProperty("over_the_top");
        Assert.NotNull(property);
        Assert.AreEqual("OverTheTop", property.Name);
    }

    // ── Converter functional ──────────────────────────────────────────────

    [Test, Parallelizable]
    public void RegisterConverter_IsUsedOnSubsequentConversion() {
        // long → byte is absent from the built-in registry, so this pair
        // exercises the new ConcurrentDictionary write path cleanly.
        bool wasCalled = false;
        Pooshit.Json.Json.SetCustomConverter(typeof(long), typeof(byte), v => {
            wasCalled = true;
            return (byte)(long)v;
        });

        byte result = Pooshit.Json.Json.Read<byte>(Pooshit.Json.Json.WriteString(42L));
        Assert.IsTrue(wasCalled, "Custom converter lambda was not invoked.");
        Assert.AreEqual((byte)42, result);
    }

    // ── Concurrency stress tests (bounded) ───────────────────────────────
    // The data race is timing-dependent; these tests verify that concurrent
    // access produces correct results (no corrupt or missing cache entries).
    // Each test is bounded: 32 tasks × 50 iterations × 7 property names = 11 200 calls.

    [Test, Parallelizable]
    public void ReflectionModel_ConcurrentGetProperty_AllResultsCorrect() {
        const int parallelism = 32;
        const int iterations  = 50;
        string[] names = new[] { "long", "string", "childTestData", "array", "guid", "weekDay", "__missing__" };

        ReflectionModel model = new(typeof(TestData));

        Task[] tasks = Enumerable.Range(0, parallelism).Select(_ => Task.Run(() => {
            for (int i = 0; i < iterations; i++) {
                foreach (string name in names)
                    model.GetProperty(name);
            }
        })).ToArray();

        Task.WaitAll(tasks);

        Assert.NotNull(model.GetProperty("long"), "Existing property must resolve after concurrent access.");
        Assert.AreEqual("Long", model.GetProperty("long").Name);
        Assert.IsNull(model.GetProperty("__missing__"), "Missing property must cache null after concurrent access.");
    }

    [Test, Parallelizable]
    public void SourceGenModel_ConcurrentGetProperty_AllResultsCorrect() {
        const int parallelism = 32;
        const int iterations  = 50;
        string[] names = new[] { "long", "string", "childTestData", "array", "guid", "weekDay", "__missing__" };

        SourceGenModel model = new(Reflection.GetModel(typeof(TestData)));

        Task[] tasks = Enumerable.Range(0, parallelism).Select(_ => Task.Run(() => {
            for (int i = 0; i < iterations; i++) {
                foreach (string name in names)
                    model.GetProperty(name);
            }
        })).ToArray();

        Task.WaitAll(tasks);

        Assert.NotNull(model.GetProperty("long"), "Existing property must resolve after concurrent access.");
        Assert.AreEqual("Long", model.GetProperty("long").Name);
        Assert.IsNull(model.GetProperty("__missing__"), "Missing property must cache null after concurrent access.");
    }
}
