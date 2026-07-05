using System;
using System.Linq;
using System.Threading.Tasks;
using Json.Tests.Data;
using NUnit.Framework;
using Pooshit.Json;
using Pooshit.Json.Models;
using Pooshit.Reflection;

namespace Json.Tests;

[TestFixture, Parallelizable]
public class CacheThreadSafetyTests {

    [Test, Parallelizable]
    public void ReflectionModel_ResolvesKnownProperty() {
        ReflectionModel model = new(typeof(TestData));
        IPropertyInfo property = model.GetProperty("long");
        Assert.NotNull(property);
        Assert.AreEqual("Long", property.Name);
    }

    [Test, Parallelizable]
    public void ReflectionModel_ReturnsCachedInstance() {
        ReflectionModel model = new(typeof(TestData));
        IPropertyInfo first  = model.GetProperty("string");
        IPropertyInfo second = model.GetProperty("string");
        Assert.AreSame(first, second);
    }

    [Test, Parallelizable]
    public void ReflectionModel_CachesNullForUnknownProperty() {
        ReflectionModel model = new(typeof(TestData));
        IPropertyInfo first  = model.GetProperty("__no_such_property__");
        IPropertyInfo second = model.GetProperty("__no_such_property__");
        Assert.IsNull(first);
        Assert.IsNull(second);
    }

    [Test, Parallelizable]
    public void ReflectionModel_ResolvesDataMemberName() {
        ReflectionModel model = new(typeof(SnakeData));
        IPropertyInfo property = model.GetProperty("over_the_top");
        Assert.NotNull(property);
        Assert.AreEqual("OverTheTop", property.Name);
    }

    [Test, Parallelizable]
    public void SourceGenModel_ResolvesKnownProperty() {
        SourceGenModel model = new(Reflection.GetModel(typeof(TestData)));
        IPropertyInfo property = model.GetProperty("long");
        Assert.NotNull(property);
        Assert.AreEqual("Long", property.Name);
    }

    [Test, Parallelizable]
    public void SourceGenModel_ReturnsCachedInstance() {
        SourceGenModel model = new(Reflection.GetModel(typeof(TestData)));
        IPropertyInfo first  = model.GetProperty("string");
        IPropertyInfo second = model.GetProperty("string");
        Assert.AreSame(first, second);
    }

    [Test, Parallelizable]
    public void SourceGenModel_CachesNullForUnknownProperty() {
        SourceGenModel model = new(Reflection.GetModel(typeof(TestData)));
        IPropertyInfo first  = model.GetProperty("__no_such_property__");
        IPropertyInfo second = model.GetProperty("__no_such_property__");
        Assert.IsNull(first);
        Assert.IsNull(second);
    }

    [Test, Parallelizable]
    public void SourceGenModel_ResolvesDataMemberName() {
        SourceGenModel model = new(Reflection.GetModel(typeof(SnakeData)));
        IPropertyInfo property = model.GetProperty("over_the_top");
        Assert.NotNull(property);
        Assert.AreEqual("OverTheTop", property.Name);
    }

    [Test, Parallelizable]
    [Description("long→byte is absent from the built-in converter registry, so this pair exercises the ConcurrentDictionary write path cleanly.")]
    public void RegisterConverter_IsUsedOnSubsequentConversion() {
        bool wasCalled = false;
        Pooshit.Json.Json.SetCustomConverter(typeof(long), typeof(byte), v => {
            wasCalled = true;
            return (byte)(long)v;
        });

        byte result = Pooshit.Json.Json.Read<byte>(Pooshit.Json.Json.WriteString(42L));
        Assert.IsTrue(wasCalled, "Custom converter lambda was not invoked.");
        Assert.AreEqual((byte)42, result);
    }

    [Test, Parallelizable]
    [Description("Verifies correct results under concurrent access; the race is timing-dependent and cannot be forced. Bounded: 32 tasks × 50 iterations × 7 property names = 11 200 calls.")]
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
    [Description("Verifies correct results under concurrent access; the race is timing-dependent and cannot be forced. Bounded: 32 tasks × 50 iterations × 7 property names = 11 200 calls.")]
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
