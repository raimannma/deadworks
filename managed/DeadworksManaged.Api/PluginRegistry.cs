namespace DeadworksManaged.Api;

/// <summary>Provides read-only access to the list of currently loaded plugins.</summary>
public static class PluginRegistry
{
    internal static Func<IReadOnlyList<string>>? Resolve;

    /// <summary>Returns the names of all currently loaded plugins.</summary>
    public static IReadOnlyList<string> GetLoadedPluginNames()
    {
        return Resolve?.Invoke() ?? [];
    }
}
