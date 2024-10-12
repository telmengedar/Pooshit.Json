﻿using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Utf8Json;

namespace Benchmark;

[SimpleJob(RuntimeMoniker.Net80)]
public class ReadBenchmark {
        
    [Benchmark]
    public void ReadMapUtf8() {
        using Stream data = typeof(Utf8Benchmark).Assembly.GetManifestResourceStream("Benchmark.Data.map.json");
        JsonSerializer.Deserialize<object>(data);
    }

    [Benchmark]
    public void ReadMapNC() {
        using Stream data = typeof(JsonBenchmark).Assembly.GetManifestResourceStream("Benchmark.Data.map.json");
        Pooshit.Json.Json.Read(data);
    }
}