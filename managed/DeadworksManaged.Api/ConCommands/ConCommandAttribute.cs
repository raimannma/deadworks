namespace DeadworksManaged.Api;

/// <summary>Marks a method as a console command handler with signature <c>void Handler(ConCommandContext ctx)</c>.</summary>
[Obsolete("Use [Command] instead.", error: false)]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class ConCommandAttribute : Attribute
{
    public string Name { get; }
    public string Description { get; set; } = "";
    public bool ServerOnly { get; set; }

    public ConCommandAttribute(string name) => Name = name;
}
