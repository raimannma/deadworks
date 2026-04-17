using DeadworksManaged.Api;

namespace AutoRestartPlugin;

public class AutoRestartConfig : IConfig
{
	/// <summary>Restart interval in minutes.</summary>
	public int IntervalMinutes { get; set; } = 60;

	public void Validate()
	{
		if (IntervalMinutes < 1) IntervalMinutes = 1;
	}
}

public class AutoRestartPlugin : DeadworksPluginBase
{
	public override string Name => "AutoRestart";

	[PluginConfig]
	public AutoRestartConfig Config { get; set; } = new();

	private IHandle? _restartSequence;

	public override void OnLoad(bool isReload)
	{
		Console.WriteLine("[AutoRestart] Loaded");
	}

	public override void OnStartupServer()
	{
		StartRestartSequence();
	}

	public override void OnConfigReloaded()
	{
		_restartSequence?.Cancel();
		StartRestartSequence();
	}

	public override void OnUnload()
	{
		_restartSequence?.Cancel();
		Console.WriteLine("[AutoRestart] Unloaded");
	}

	private void StartRestartSequence()
	{
		var intervalMin = Config.IntervalMinutes;
		Console.WriteLine($"[AutoRestart] Scheduling restart in {intervalMin} minutes");

		var notifications = new List<(int SecondsRemaining, string Message)>();

		if (intervalMin >= 11)
			notifications.Add((600, "Map restart in 10 minutes"));
		if (intervalMin >= 6)
			notifications.Add((300, "Map restart in 5 minutes"));
		if (intervalMin >= 2)
			notifications.Add((60, "Map restart in 1 minute"));

		for (int i = 10; i >= 1; i--)
			notifications.Add((i, $"Map restart in {i} second{(i == 1 ? "" : "s")}"));

		notifications.Sort((a, b) => b.SecondsRemaining.CompareTo(a.SecondsRemaining));

		var totalSeconds = intervalMin * 60;
		var notifIndex = 0;
		var elapsedSeconds = 0;

		_restartSequence = Timer.Sequence(step =>
		{
			if (notifIndex < notifications.Count)
			{
				var (secondsRemaining, message) = notifications[notifIndex];
				var targetElapsed = totalSeconds - secondsRemaining;

				if (elapsedSeconds >= targetElapsed)
				{
					Chat.PrintToChatAll(message);
					Console.WriteLine($"[AutoRestart] {message}");
					notifIndex++;

					if (notifIndex < notifications.Count)
					{
						var nextSecondsRemaining = notifications[notifIndex].SecondsRemaining;
						var waitSeconds = secondsRemaining - nextSecondsRemaining;
						elapsedSeconds += waitSeconds;
						return step.Wait(waitSeconds.Seconds());
					}

					DoRestart();
					return step.Done();
				}

				var waitUntilNext = targetElapsed - elapsedSeconds;
				elapsedSeconds = targetElapsed;
				return step.Wait(waitUntilNext.Seconds());
			}

			DoRestart();
			return step.Done();
		}).CancelOnMapChange();
	}

	private void DoRestart()
	{
		var map = Server.MapName;
		Console.WriteLine($"[AutoRestart] Restarting - changelevel {map}");
		Server.ExecuteCommand($"changelevel {map}");
	}
}
