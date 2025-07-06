namespace Pooshit.Json;

/// <summary>
/// behavior when encountering byte arrays in json data to write
/// </summary>
public enum ByteArrayBehavior {
	
	/// <summary>
	/// keep byte array as is
	/// </summary>
	Keep,
	
	/// <summary>
	/// replace byte array with null
	/// </summary>
	Strip,
	
	/// <summary>
	/// write base64 representation of byte array
	/// </summary>
	Base64
}