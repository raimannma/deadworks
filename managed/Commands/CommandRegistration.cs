using System.Reflection;
using System.Runtime.ExceptionServices;
using DeadworksManaged.Api;

namespace DeadworksManaged.Commands;

internal static class CommandRegistration
{
    public static void RegisterPluginCommands(
        string normalizedPath,
        List<IDeadworksPlugin> plugins,
        HandlerRegistry<string, Func<ChatCommandContext, HookResult>> chatRegistry)
    {
        foreach (var plugin in plugins)
        {
            var methods = plugin.GetType().GetMethods(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var method in methods)
            {
                var attrs = method.GetCustomAttributes<CommandAttribute>();
                foreach (var attr in attrs)
                {
                    if (attr.ChatOnly && attr.ConsoleOnly)
                    {
                        Console.WriteLine(
                            $"[CommandRegistration] {plugin.Name}.{method.Name}: ChatOnly and ConsoleOnly both set — skipping");
                        continue;
                    }

                    CommandBinder.Plan plan;
                    try
                    {
                        plan = CommandBinder.Build(method, attr.Names[0]);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[CommandRegistration] {plugin.Name}.{method.Name}: {ex.Message}");
                        continue;
                    }

                    foreach (var name in new HashSet<string>(attr.Names, StringComparer.OrdinalIgnoreCase))
                    {
                        if (!attr.ConsoleOnly)
                            RegisterChat(normalizedPath, plugin, method, plan, name, attr, chatRegistry);

                        if (!attr.ChatOnly)
                            RegisterConsole(normalizedPath, plugin, method, plan, name, attr);
                    }
                }
            }
        }
    }

    private static void RegisterChat(
        string normalizedPath,
        IDeadworksPlugin plugin,
        MethodInfo method,
        CommandBinder.Plan plan,
        string name,
        CommandAttribute attr,
        HandlerRegistry<string, Func<ChatCommandContext, HookResult>> chatRegistry)
    {
        var namedPlan = name == plan.Name ? plan : new CommandBinder.Plan
        {
            Name = name,
            Slots = plan.Slots,
            HasCaller = plan.HasCaller,
            CallerNullable = plan.CallerNullable
        };

        Func<ChatCommandContext, HookResult> handler = ctx =>
        {
            if (attr.ServerOnly)
                return HookResult.Continue;

            var resultOnSuccess = (ctx.Prefix == '!' && !attr.SuppressChat)
                ? HookResult.Continue
                : HookResult.Handled;

            void reply(string msg) => ReplyViaChat(ctx.Controller, msg);

            var argString = ctx.Args.Length > 0 ? string.Join(" ", ctx.Args) : "";
            var tokens = CommandTokenizer.Tokenize(argString);

            if (!CommandBinder.TryBind(namedPlan, tokens, ctx.Controller, out var boundArgs, out var error, out var silentSkip))
            {
                if (silentSkip)
                    return resultOnSuccess;
                if (error != null)
                    reply(error);
                return resultOnSuccess;
            }

            Invoke(plugin, method, boundArgs, reply);
            return resultOnSuccess;
        };

        chatRegistry.AddForPlugin(normalizedPath, name, handler);
        PluginRegistrationTracker.Add(normalizedPath, "chat", $"/{name}", attr.Description, attr.Hidden);
        Console.WriteLine($"[CommandRegistration] Registered chat command: {plugin.Name} -> /{name}");
    }

    private static void RegisterConsole(
        string normalizedPath,
        IDeadworksPlugin plugin,
        MethodInfo method,
        CommandBinder.Plan plan,
        string name,
        CommandAttribute attr)
    {
        var conName = "dw_" + name;
        var namedPlan = conName == plan.Name ? plan : new CommandBinder.Plan
        {
            Name = conName,
            Slots = plan.Slots,
            HasCaller = plan.HasCaller,
            CallerNullable = plan.CallerNullable
        };

        Action<ConCommandContext> handler = ctx =>
        {
            if (attr.ServerOnly && !ctx.IsServerCommand)
                return;

            void reply(string msg) => ReplyViaConsole(ctx.Controller, msg);

            var argString = ctx.Args.Length > 1
                ? string.Join(" ", ctx.Args, 1, ctx.Args.Length - 1)
                : "";
            var tokens = CommandTokenizer.Tokenize(argString);

            if (!CommandBinder.TryBind(namedPlan, tokens, ctx.Controller, out var boundArgs, out var error, out var silentSkip))
            {
                if (silentSkip)
                    return;
                if (error != null)
                    reply(error);
                return;
            }

            Invoke(plugin, method, boundArgs, reply);
        };

        ConCommandManager.RegisterExternal(normalizedPath, conName, attr.Description, serverOnly: false, handler, attr.Hidden);
        Console.WriteLine($"[CommandRegistration] Registered console command: {plugin.Name} -> {conName}{(attr.ServerOnly ? " (server-only)" : "")}");
    }

    private static void Invoke(
        IDeadworksPlugin plugin,
        MethodInfo method,
        object?[] boundArgs,
        Action<string> reply)
    {
        try
        {
            method.Invoke(plugin, boundArgs);
        }
        catch (TargetInvocationException tie) when (tie.InnerException is CommandException cex)
        {
            reply(cex.Message);
        }
        catch (TargetInvocationException tie) when (tie.InnerException != null)
        {
            ExceptionDispatchInfo.Capture(tie.InnerException).Throw();
        }
    }

    private static void ReplyViaChat(CCitadelPlayerController? to, string message)
    {
        if (to != null)
            Chat.PrintToChat(to, message);
    }

    private static void ReplyViaConsole(CCitadelPlayerController? to, string message)
    {
        if (to != null)
            to.PrintToConsole(message);
        else
            Console.WriteLine(message);
    }
}
