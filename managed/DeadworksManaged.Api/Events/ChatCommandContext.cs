namespace DeadworksManaged.Api;

/// <summary>Provides context for an invoked chat command, including the originating message, parsed command name, and arguments.</summary>
public sealed class ChatCommandContext
{
    /// <summary>The raw chat message that triggered this command.</summary>
    public ChatMessage Message { get; }

    /// <summary>The matched command string (e.g. <c>"!mycommand"</c>).</summary>
    public string Command { get; }

    /// <summary>Arguments following the command, split by whitespace.</summary>
    public string[] Args { get; }

    /// <summary>The prefix character that introduced this command (<c>'/'</c> or <c>'!'</c>).</summary>
    public char Prefix { get; }

    /// <summary>The player controller who sent the command, or <see langword="null"/> if unavailable.</summary>
    public CCitadelPlayerController? Controller => Message.Controller;

    internal ChatCommandContext(ChatMessage message, string command, string[] args, char prefix)
    {
        Message = message;
        Command = command;
        Args = args;
        Prefix = prefix;
    }
}
