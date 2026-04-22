namespace DeadworksManaged.Api;

/// <summary>Thrown from a <see cref="CommandAttribute"/> handler to send <see cref="Exception.Message"/> to the caller.</summary>
public sealed class CommandException : Exception
{
    public CommandException(string message) : base(message) { }
    public CommandException(string message, Exception inner) : base(message, inner) { }
}
