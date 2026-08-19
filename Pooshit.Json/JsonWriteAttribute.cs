using System;

namespace Pooshit.Json;

/// <summary>
/// marks a property without a public setter for emission on write
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false)]
public class JsonWriteAttribute : Attribute {
}
