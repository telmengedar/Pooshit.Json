using System.Collections.Generic;
using System.Linq;

namespace Pooshit.Json.Extensions;

#if NETSTANDARD2_1

/// <summary>
/// extensions for async enumerables
/// </summary>
public static class AsyncEnumerableExtensions {
    
    /// <summary>
    /// checks an instance whether it is an <see cref="IAsyncEnumerable{T}"/>
    /// </summary>
    /// <param name="instance">instance to check</param>
    /// <returns>true if type is an <see cref="IAsyncEnumerable{T}"/>, false otherwise</returns>
    public static bool IsAsyncEnumerable(this object instance) {
        if (instance == null)
            return false;
        
        return instance.GetType().GetInterfaces().Any(x =>
                                                          x.IsGenericType &&
                                                          x.GetGenericTypeDefinition() == typeof(IAsyncEnumerable<>));
    }
}

#endif
