using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Json.Tests.Data;
using NUnit.Framework;
using Pooshit.Json;

namespace Json.Tests;

[TestFixture, Parallelizable]
public class JsonReaderTests {

    [Test, Parallelizable]
    public void SerializeEnumInObject() {
        string serialized = Pooshit.Json.Json.WriteString(new TestData {
                                                                               WeekDay = DayOfWeek.Wednesday
                                                                           });
        TestData deserialized = Pooshit.Json.Json.Read<TestData>(serialized);
        Assert.AreEqual(DayOfWeek.Wednesday, deserialized.WeekDay);
    }

    [Test, Parallelizable]
    public void SerializeEnumInObjectCamelCased() {
        string serialized = Pooshit.Json.Json.WriteString(new TestData {
                                                                           WeekDay = DayOfWeek.Wednesday
                                                                       }, new() {
                                                                                    ExcludeNullProperties = true,
                                                                                    NamingStrategy = NamingStrategies.CamelCase
                                                                                });
        TestData deserialized = Pooshit.Json.Json.Read<TestData>(serialized);
        Assert.AreEqual(DayOfWeek.Wednesday, deserialized.WeekDay);
    }

    [Test, Parallelizable]
    public void ReadArrayOfArray() {
        object[][] result = Pooshit.Json.Json.Read<object[][]>("[[1,2],[3,4],[5,6]]");
        Assert.AreEqual(3, result.Length);
        Assert.That(result.All(i => i.Length == 2));
    }
        
    [Test, Parallelizable]
    public void ReadEncodedString() {
        string data = Pooshit.Json.Json.Read<string>("\"1234567890\"");
        Assert.AreEqual("1234567890", data);
    }
        
    [Test, Parallelizable]
    public void ReadDataMember() {
        SnakeData snakedata = Pooshit.Json.Json.Read<SnakeData>("{\"over_the_top\": 99}");
        Assert.AreEqual(99, snakedata.OverTheTop);
    }
        
    [Test, Parallelizable]
    public void ReadProperType() {
        object result = Pooshit.Json.Json.Read("97");
        Assert.AreEqual(97, result);
    }

    [TestCase("Json.Tests.Data.map.json")]
    [Parallelizable]
    public void ReadValidJson(string resource) {
        using Stream datastream = typeof(JsonReaderTests).Assembly.GetManifestResourceStream(resource);
        object result = Pooshit.Json.Json.Read(datastream);
        Assert.NotNull(result);
    }

    [TestCase("Json.Tests.Data.map.json")]
    [Parallelizable]
    public async Task ReadValidJsonAsync(string resource) {
        using Stream datastream = typeof(JsonReaderTests).Assembly.GetManifestResourceStream(resource);
        object result = await Pooshit.Json.Json.ReadAsync(datastream);
        Assert.NotNull(result);
    }

    [Test, Parallelizable]
    public void ReadEmptyObject() {
        TestData testdata = Pooshit.Json.Json.Read<TestData>("{}");
        Assert.NotNull(testdata);
    }

    [Test, Parallelizable]
    public async Task ReadEmptyObjectAsync() {
        TestData testdata = await Pooshit.Json.Json.ReadAsync<TestData>("{}");
        Assert.NotNull(testdata);
    }

    [Test, Parallelizable]
    public void ReadGuid() {
        Guid guid = Pooshit.Json.Json.Read<Guid>(Pooshit.Json.Json.WriteString(Guid.Empty));
        Assert.AreEqual(Guid.Empty, guid);
    }

    [Test, Parallelizable]
    public async Task ReadGuidAsync() {
        Guid guid = await Pooshit.Json.Json.ReadAsync<Guid>(await Pooshit.Json.Json.WriteStringAsync(Guid.Empty));
        Assert.AreEqual(Guid.Empty, guid);
    }

    [Test, Parallelizable]
    public void ReadDateTime() {
        DateTime result = Pooshit.Json.Json.Read<DateTime>("\"2020-12-31T00:00:00.0000000\"");
        Assert.AreEqual(2020, result.Year);
        Assert.AreEqual(12, result.Month);
        Assert.AreEqual(31, result.Day);
    }

    [Test, Parallelizable]
    public void ReadEstTime() {
        DateTime result = Pooshit.Json.Json.Read<DateTime>("\"2020-06-01 03:08am\"");
        Assert.AreEqual(2020, result.Year);
        Assert.AreEqual(6, result.Month);
        Assert.AreEqual(1, result.Day);
        Assert.AreEqual(3, result.Hour);
        Assert.AreEqual(8, result.Minute);
    }

    [Test, Parallelizable]
    public void ReadEstTimePm() {
        DateTime result = Pooshit.Json.Json.Read<DateTime>("\"2020-06-01 03:08pm\"");
        Assert.AreEqual(2020, result.Year);
        Assert.AreEqual(6, result.Month);
        Assert.AreEqual(1, result.Day);
        Assert.AreEqual(15, result.Hour);
        Assert.AreEqual(8, result.Minute);
    }

    [Test, Parallelizable]
    public void ReadTimespan() {
        TimeSpan result = Pooshit.Json.Json.Read<TimeSpan>("\"04:34:22\"");
        Assert.AreEqual(4, result.Hours);
        Assert.AreEqual(34, result.Minutes);
        Assert.AreEqual(22, result.Seconds);
    }

    [Test, Parallelizable]
    public async Task ReadTimespanAsync() {
        TimeSpan result = await Pooshit.Json.Json.ReadAsync<TimeSpan>("\"04:34:22\"");
        Assert.AreEqual(4, result.Hours);
        Assert.AreEqual(34, result.Minutes);
        Assert.AreEqual(22, result.Seconds);
    }

    [Test, Parallelizable]
    public void ReadGuidProperty() {
        TestData testdata = Pooshit.Json.Json.Read<TestData>("{\"guid\":\"00000000-0000-0000-0000-000000000000\"}");
        Assert.NotNull(testdata);
        Assert.AreEqual(Guid.Empty, testdata.Guid);
    }
        
    [Test, Parallelizable]
    public async Task ReadGuidPropertyAsync() {
        TestData testdata = await Pooshit.Json.Json.ReadAsync<TestData>("{\"guid\":\"00000000-0000-0000-0000-000000000000\"}");
        Assert.NotNull(testdata);
        Assert.AreEqual(Guid.Empty, testdata.Guid);
    }

    [TestCase("Json.Tests.Data.testarray.json")]
    [TestCase("Json.Tests.Data.emptyobjectproperty.json")]
    [TestCase("Json.Tests.Data.emptyobjectpropertyinarray.json")]
    [TestCase("Json.Tests.Data.campaign.json")]
    [Parallelizable]
    public void ReadData(string resource) {
        object result = Pooshit.Json.Json.Read(typeof(JsonReaderTests).Assembly.GetManifestResourceStream(resource));
    }
        
    [TestCase("Json.Tests.Data.testarray.json")]
    [TestCase("Json.Tests.Data.emptyobjectproperty.json")]
    [TestCase("Json.Tests.Data.emptyobjectpropertyinarray.json")]
    [TestCase("Json.Tests.Data.campaign.json")]
    [Parallelizable]
    public async Task ReadDataAsync(string resource) {
        await Pooshit.Json.Json.ReadAsync(typeof(JsonReaderTests).Assembly.GetManifestResourceStream(resource));
    }

    [TestCase("Json.Tests.Data.testarray.json")]
    [TestCase("Json.Tests.Data.emptyobjectproperty.json")]
    [TestCase("Json.Tests.Data.emptyobjectpropertyinarray.json")]
    [TestCase("Json.Tests.Data.campaign.json")]
    [Parallelizable]
    public async Task ReadAsyncFromAutoClosingStream(string resource) {
        await using AutoclosingStream acl = new(typeof(JsonReaderTests).Assembly.GetManifestResourceStream(resource)); 
        await Pooshit.Json.Json.ReadAsync(acl);
    }
        
    [Test, Parallelizable]
    public void ReadAsDictionaryInterface() {
        Pooshit.Json.Json.Read<IDictionary<string, object>>("{\"name\":\"Günther\"}");
    }

    [Test, Parallelizable]
    public void ReadTypeFromStructure() {
        object structure = Pooshit.Json.Json.Read<object>("{\"long\":4,\"childTestData\":{\"string\": \"abc\"},\"array\":[4,3,3,9], \"objectArray\":[{\"string\": \"abc1\"},{\"string\": \"abc2\"}]}");
        TestData data = Pooshit.Json.Json.Read<TestData>(structure);
        Assert.NotNull(data);
        Assert.NotNull(data.ChildTestData);
        Assert.NotNull(data.Array);
        Assert.NotNull(data.ObjectArray);
        Assert.AreEqual(4, data.Long);
        Assert.AreEqual("abc", data.ChildTestData.String);
        Assert.That(new[] { 4, 3, 3, 9 }.SequenceEqual(data.Array));
        Assert.AreEqual(2, data.ObjectArray.Length);
        Assert.AreEqual("abc1", data.ObjectArray[0].String);
        Assert.AreEqual("abc2", data.ObjectArray[1].String);
    }

    [Test, Parallelizable]
    public void ReadTypeFromStructureWithError() {
        object structure = Pooshit.Json.Json.Read<object>("{\"date\":\"-0001-11-30T12:00:00+02:00\"}");
        TestData data = Pooshit.Json.Json.Read<TestData>(structure, (exception, type, value) => DateTime.Now);
        Assert.NotNull(data);
        Assert.That(data.Date, Is.GreaterThan(new DateTime(2000,1,1)));
    }

    [Test, Parallelizable]
    public void ReadArrayFromStructure() {
        object structure = Pooshit.Json.Json.Read<object>("[{\"string\": \"abc1\"},{\"string\": \"abc2\"}]");
        TestData[] data = Pooshit.Json.Json.Read<TestData[]>(structure);
        Assert.NotNull(data);
        Assert.AreEqual(2, data.Length);
        Assert.AreEqual("abc1", data[0].String);
        Assert.AreEqual("abc2", data[1].String);
    }

    [Test, Parallelizable]
    public void ReadDictionaryInterface() {
        object structure = Pooshit.Json.Json.Read<object>("{\"dictionary\": {\"name\": 8}}");
        DataWithDictionary dictionary = Pooshit.Json.Json.Read<DataWithDictionary>(structure);
        Assert.NotNull(dictionary);
        Assert.NotNull(dictionary.Dictionary);
        Assert.AreEqual(8L, dictionary.Dictionary["name"]);
    }

    [Test, Parallelizable]
    [Timeout(1000)]
    public async Task ReadValueAsync() {
        string json = Pooshit.Json.Json.WriteString(1);
        int value = await Pooshit.Json.Json.ReadAsync<int>(json);
        Assert.AreEqual(value, 1);
    }

    [Test, Parallelizable]
    public void IgnoreNullFields() {
        string data = "{\"long\":null}";
        TestData deserialized = Pooshit.Json.Json.Read<TestData>(data);
        Assert.AreEqual(0, deserialized.Long);
    }
    
    [Test, Parallelizable]
    public void DeserializeComplexType() {
        var deserialized = Pooshit.Json.Json.Read<Dictionary<string,Dictionary<string,TestData>>>("{}");
        Assert.NotNull(deserialized);
    }

    [Test, Parallelizable]
    public void DeserializeComplexTypeWithData() {
        var deserialized = Pooshit.Json.Json.Read<Dictionary<string,Dictionary<string,TestData>>>("{\"test\":{\"test\":{\"long\":7}}}");
        Assert.NotNull(deserialized);
        Assert.AreEqual(7, JPath.Select<long>(deserialized, "test/test/Long"));
    }
    
    [Test, Parallelizable]
    public async Task DeserializeComplexTypeWithDataAsync() {
        var deserialized = await Pooshit.Json.Json.ReadAsync<Dictionary<string,Dictionary<string,TestData>>>("{\"test\":{\"test\":{\"long\":7}}}");
        Assert.NotNull(deserialized);
        Assert.AreEqual(7, JPath.Select<long>(deserialized, "test/test/Long"));
    }

    [Test, Parallelizable]
    public void NegativeIsReadAsLong() {
        object deserialized = Pooshit.Json.Json.Read("-823");
        Assert.That(deserialized is long);
    }
}