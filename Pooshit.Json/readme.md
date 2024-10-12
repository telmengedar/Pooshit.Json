# P00Sh:7 Json Library
This is a minimalistic Json library which aims to be fast and not huge in size. It is tested in real world scenarios but could still contain some bugs.

The Library was benchmarked against Utf8Json and System.Text.Json and seems to perform pretty well.

## Serialize to Json
To serialize something to json one of the static **Write** methods is to be used.

```csharp
Json.WriteString(...)
```

```csharp
Json.Write(..., <stream>)
```

To change serialization format like property naming the **Write** methods accept an argument of type **JsonOptions** in which options like camel casing or null value handling can be specified.

## Deserialize from Json
To deserialize a json formatted string or stream one of the static **Read** methods is to be used.

```csharp
var deserialized=Json.Read(data)
```
```csharp
var deserialized=Json.Read<T>(data)
```