using System.Reflection;
using DeadworksManaged.Api;

namespace DeadworksManaged;

internal static partial class PluginLoader
{
    // --- Chat message dispatch (with command routing) ---

    public static HookResult DispatchChatMessage(ChatMessage message)
    {
        var result = HookResult.Continue;

        var text = message.ChatText.Trim();
        if (text.Length > 1 && (text[0] == '/' || text[0] == '!'))
        {
            var prefix = text[0];
            var parts = text[1..].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var commandName = parts[0];
            var args = parts.Length > 1 ? parts[1..] : [];

            List<Func<ChatCommandContext, HookResult>>? handlers;
            lock (_lock)
            {
                handlers = _chatCommandRegistry.Snapshot(commandName);
            }

            if (handlers != null)
            {
                var ctx = new ChatCommandContext(message, commandName, args, prefix);
                foreach (var handler in handlers)
                {
                    try
                    {
                        var hr = handler(ctx);
                        if (hr > result) result = hr;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[PluginLoader] Chat command handler for '/{commandName}' threw: {ex.Message}");
                    }
                }

                if (result > HookResult.Continue)
                    return result;
            }
        }

        // Fall through to plugin OnChatMessage
        return DispatchToPluginsWithResult(p => p.OnChatMessage(message), nameof(IDeadworksPlugin.OnChatMessage));
    }

    // --- Chat command registration ---

    private static void RegisterPluginChatCommands(string normalizedPath, List<IDeadworksPlugin> plugins)
    {
        foreach (var plugin in plugins)
        {
            var methods = plugin.GetType().GetMethods(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var method in methods)
            {
#pragma warning disable CS0618 // ChatCommandAttribute is obsolete; intentionally scanned for back-compat
                var attrs = method.GetCustomAttributes<ChatCommandAttribute>();
#pragma warning restore CS0618
                foreach (var attr in attrs)
                {
                    var del = (Func<ChatCommandContext, HookResult>)Delegate.CreateDelegate(
                        typeof(Func<ChatCommandContext, HookResult>), plugin, method);

                    _chatCommandRegistry.AddForPlugin(normalizedPath, attr.Command, del);
                    PluginRegistrationTracker.Add(normalizedPath, "chat", $"/{attr.Command}");
                    Console.WriteLine($"[PluginLoader] Registered chat command: {plugin.Name} -> /{attr.Command}");
                }
            }
        }
    }
}
