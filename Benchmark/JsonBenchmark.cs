﻿using System;
using System.IO;
using Benchmark.Data;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Pooshit.Json;

namespace Benchmark;

[SimpleJob(RuntimeMoniker.Net80)]
public class JsonBenchmark {
    readonly JsonOptions camelcased = new() {
                                                ExcludeNullProperties = true,
                                                NamingStrategy = NamingStrategies.CamelCase
                                            };
        
    [Benchmark]
    public void WriteString() {
        Pooshit.Json.Json.WriteString("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.");
    }

    [Benchmark]
    public void WriteStream() {
        using MemoryStream ms=new MemoryStream();
        Pooshit.Json.Json.Write("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.", ms);
    }

    [Benchmark]
    public void WriteObjectString() {
        Pooshit.Json.Json.WriteString(new TestData {
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
    public void WriteObjectStringCamelCased() {
        Pooshit.Json.Json.WriteString(new TestData {
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
    public void WriteObjectStream() {
        using MemoryStream ms=new MemoryStream();
        Pooshit.Json.Json.Write(new TestData {
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
                                                 }, ms);
    }
}