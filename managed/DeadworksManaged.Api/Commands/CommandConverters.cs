using System.Collections.Concurrent;

namespace DeadworksManaged.Api;

/// <summary>Per-type string parsers for <see cref="CommandAttribute"/>. Register from <c>OnLoad</c>.</summary>
public static class CommandConverters
{
    private static readonly ConcurrentDictionary<Type, Func<string, object>> _converters = new();

    /// <summary>Registers a parser for <typeparamref name="T"/>. Overwrites any prior registration.</summary>
    public static void Register<T>(Func<string, T> parser)
        => _converters[typeof(T)] = s => parser(s)!;

    public static bool Unregister<T>() => _converters.TryRemove(typeof(T), out _);

    internal static bool TryConvert(string token, Type type, out object? value)
    {
        if (_converters.TryGetValue(type, out var fn))
        {
            value = fn(token);
            return true;
        }
        value = null;
        return false;
    }
}
