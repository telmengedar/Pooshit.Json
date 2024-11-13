using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Pooshit.Json.Extensions;

#if NETSTANDARD2_1

/// <summary>
/// extensions for async enumerables
/// </summary>
public static class AsyncEnumerableExtensions {
    static BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
    
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

    /// <summary>
    /// get async enumerator methods from async enumerable instance
    /// </summary>
    /// <param name="enumerable">async enumerable instance</param>
    /// <returns>enumerator methods</returns>
    public static Tuple<IAsyncDisposable, MethodInfo, PropertyInfo> GetAsyncEnumerator(this object enumerable) {
        MethodInfo enumeratorInfo = enumerable.GetType().GetMethod("GetAsyncEnumerator", bindingFlags);
        if (enumeratorInfo == null) {
            enumeratorInfo = enumerable.GetType().GetMethods(bindingFlags)
                                       .FirstOrDefault(m => m.Name.EndsWith(".GetAsyncEnumerator"));
        }

        if (enumeratorInfo == null)
            return null;
        
        object enumerator = enumeratorInfo.Invoke(enumerable, [default(CancellationToken)]);

        MethodInfo moveNextAsync = enumerator.GetType().GetMethod("MoveNextAsync");
        if (moveNextAsync == null) {
            moveNextAsync = enumerator.GetType().GetMethods(bindingFlags)
                                       .FirstOrDefault(m => m.Name.EndsWith(".MoveNextAsync"));
        }
        
        PropertyInfo getCurrent = enumerator.GetType().GetProperty("Current");
        if (getCurrent == null) {
            getCurrent = enumerator.GetType().GetProperties(bindingFlags)
                                   .FirstOrDefault(p => p.Name.EndsWith(".Current"));
        }

        if (moveNextAsync == null || getCurrent == null)
            return null;
        
        return new(enumerator as IAsyncDisposable, moveNextAsync, getCurrent);
    }
}

#endif
