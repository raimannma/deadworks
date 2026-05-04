using Microsoft.Extensions.Logging;
using DeadworksManaged.Api;
using DeadworksManaged.Telemetry;

namespace DeadworksManaged;

internal static partial class PluginLoader
{
    // --- Entity IO hooks ---

    private static IHandle OnEntityIOHookInput(string designerName, string inputName, Action<EntityInputEvent> handler)
    {
        var key = $"{designerName}:{inputName}";
        lock (_lock)
        {
            if (!_inputHooks.TryGetValue(key, out var list))
            {
                list = new List<Action<EntityInputEvent>>();
                _inputHooks[key] = list;
            }
            list.Add(handler);
        }

        return new CallbackHandle(() =>
        {
            lock (_lock)
            {
                if (_inputHooks.TryGetValue(key, out var list))
                {
                    list.Remove(handler);
                    if (list.Count == 0)
                        _inputHooks.Remove(key);
                }
            }
        });
    }


    public static void DispatchEntityAcceptInput(string designerName, EntityInputEvent evt)
    {
        var key = $"{designerName}:{evt.InputName}";
        List<Action<EntityInputEvent>>? handlers;
        lock (_lock)
        {
            if (!_inputHooks.TryGetValue(key, out handlers))
                return;
            handlers = [.. handlers]; // snapshot
        }

        foreach (var handler in handlers)
        {
            try
            {
                handler(evt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Entity input hook {Key} threw", key);
            }
        }
    }
}
