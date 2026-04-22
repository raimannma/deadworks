using System.Text.Json;
using DeadworksManaged.Api;

namespace DumperPlugin;

public class DumperPlugin : DeadworksPluginBase
{
	public override string Name => "Dumper";

	private static readonly string DefaultOutputDir = Path.Combine(
		Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
		"deadlock_dumps");

	public override void OnLoad(bool isReload)
	{
		Console.WriteLine("[Dumper] Loaded. Commands: dw_cvardump");
	}

	public override void OnUnload() { }

	// --- CVar Dump ---

	[Command("cvardump",
		Description = "Dump all ConVars and ConCommands to a JSON file",
		ServerOnly = true,
		ConsoleOnly = true)]
	public void CmdCvarDump(string outputPath = "")
	{
		DoCvarDump(outputPath);
	}

	private void DoCvarDump(string outputPath)
	{
		if (string.IsNullOrWhiteSpace(outputPath))
		{
			Directory.CreateDirectory(DefaultOutputDir);
			string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
			outputPath = Path.Combine(DefaultOutputDir, $"cvardump_{timestamp}.json");
		}

		string? dir = Path.GetDirectoryName(outputPath);
		if (!string.IsNullOrEmpty(dir))
			Directory.CreateDirectory(dir);

		var convars = Server.EnumerateConVars().ToList();
		var concommands = Server.EnumerateConCommands().ToList();
		var dump = new { convars, concommands };
		var json = JsonSerializer.Serialize(dump, new JsonSerializerOptions { WriteIndented = true });
		File.WriteAllText(outputPath, json);

		Console.WriteLine($"[Dumper] Dumped {convars.Count} ConVars and {concommands.Count} ConCommands to: {outputPath}");
	}
}
