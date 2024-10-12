using System;
using System.IO;
using System.Text.Json;
using Benchmark.Data;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace Benchmark;

[SimpleJob(RuntimeMoniker.Net80)]
public class MSBenchmark {
    readonly JsonSerializerOptions camelcased = new() {
                                                          WriteIndented = true,
                                                          PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                                                      };
        
    [Benchmark]
    public void WriteUtf8String() {
        JsonSerializer.Serialize("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.");
    }

    [Benchmark]
    public void WriteUtf8StringToStream() {
        MemoryStream ms=new();
        JsonSerializer.Serialize(ms, "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.");
    }
        
    [Benchmark]
    public void WriteUtf8Object() {
        JsonSerializer.Serialize(new TestData {
                                                                   Guid = Guid.Empty,
                                                                   Decimal = 0.33m,
                                                                   Long = 4,
                                                                   String = "Hello there",
                                                                   Array = [1, 6, 23, 5, 3, 1, 77, 4, 35, 62],
                                                                   ChildTestData = new() {
                                                                                             Guid = Guid.Empty,
                                                                                             Decimal = 0.64m,
                                                                                             Long = 8,
                                                                                             String = "Hello back",
                                                                                             Array = [4, 2, 5, 5, 5, 23, 4, 5, 63, 45, 6, 72, 7],
                                                                                         }
                                                               });    
    }

    [Benchmark]
    public void WriteUtf8ObjectCamelCased() {
        JsonSerializer.Serialize(new TestData {
                                                                   Guid = Guid.Empty,
                                                                   Decimal = 0.33m,
                                                                   Long = 4,
                                                                   String = "Hello there",
                                                                   Array = [1, 6, 23, 5, 3, 1, 77, 4, 35, 62],
                                                                   ChildTestData = new() {
                                                                                             Guid = Guid.Empty,
                                                                                             Decimal = 0.64m,
                                                                                             Long = 8,
                                                                                             String = "Hello back",
                                                                                             Array = [4, 2, 5, 5, 5, 23, 4, 5, 63, 45, 6, 72, 7],
                                                                                         }
                                                               }, camelcased);
    }

    [Benchmark]
    public void WriteUtf8ObjectToStream() {
        MemoryStream ms=new();
        JsonSerializer.Serialize(ms, new TestData {
                                                                       Guid = Guid.Empty,
                                                                       Decimal = 0.33m,
                                                                       Long = 4,
                                                                       String = "Hello there",
                                                                       Array = [1, 6, 23, 5, 3, 1, 77, 4, 35, 62],
                                                                       ChildTestData = new() {
                                                                                                 Guid = Guid.Empty,
                                                                                                 Decimal = 0.64m,
                                                                                                 Long = 8,
                                                                                                 String = "Hello back",
                                                                                                 Array = [4, 2, 5, 5, 5, 23, 4, 5, 63, 45, 6, 72, 7],
                                                                                             }
                                                                   });    
    }
}