using System;
using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Json.Tests.Data;
using Utf8Json;
using Utf8Json.Resolvers;

namespace Benchmark {
    
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    public class Utf8Benchmark {
        readonly IJsonFormatterResolver camelcased = StandardResolver.ExcludeNullCamelCase;
        
        [Benchmark]
        public void WriteUtf8String() {
            JsonSerializer.ToJsonString("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.");
        }

        [Benchmark]
        public void WriteUtf8StringToStream() {
            MemoryStream ms=new MemoryStream();
            JsonSerializer.Serialize(ms, "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.");
        }
        
        [Benchmark]
        public void WriteUtf8Object() {
            JsonSerializer.ToJsonString(new TestData {
                Guid = Guid.Empty,
                Decimal = 0.33m,
                Long = 4,
                String = "Hello there",
                Array = new[] {1, 6, 23, 5, 3, 1, 77, 4, 35, 62},
                ChildTestData = new TestData {
                    Guid = Guid.Empty,
                    Decimal = 0.64m,
                    Long = 8,
                    String = "Hello back",
                    Array = new[] {4, 2, 5, 5, 5, 23, 4, 5, 63, 45, 6, 72, 7},
                }
            });    
        }

        [Benchmark]
        public void WriteUtf8ObjectCamelCased() {
            JsonSerializer.ToJsonString(new TestData {
                Guid = Guid.Empty,
                Decimal = 0.33m,
                Long = 4,
                String = "Hello there",
                Array = new[] {1, 6, 23, 5, 3, 1, 77, 4, 35, 62},
                ChildTestData = new TestData {
                    Guid = Guid.Empty,
                    Decimal = 0.64m,
                    Long = 8,
                    String = "Hello back",
                    Array = new[] {4, 2, 5, 5, 5, 23, 4, 5, 63, 45, 6, 72, 7},
                }
            }, camelcased);
        }

        [Benchmark]
        public void WriteUtf8ObjectToStream() {
            MemoryStream ms=new MemoryStream();
            JsonSerializer.Serialize(ms, new TestData {
                Guid = Guid.Empty,
                Decimal = 0.33m,
                Long = 4,
                String = "Hello there",
                Array = new[] {1, 6, 23, 5, 3, 1, 77, 4, 35, 62},
                ChildTestData = new TestData {
                    Guid = Guid.Empty,
                    Decimal = 0.64m,
                    Long = 8,
                    String = "Hello back",
                    Array = new[] {4, 2, 5, 5, 5, 23, 4, 5, 63, 45, 6, 72, 7},
                }
            });    
        }
    }
}