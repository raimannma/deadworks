using DeadworksManaged.Api;

namespace ScourgePlugin;

public class ScourgeConfig : IConfig {
	public float DurationSeconds { get; set; } = 15f;
	public int DamageIntervalMs { get; set; } = 200;
	public float DamageFraction { get; set; } = 0.005f;
	public string DamageSound { get; set; } = "Damage.Send.Crit";
	public float DamageSoundVolume { get; set; } = 0.1f;

	public void Validate() {
		if (DurationSeconds < 0.1f) DurationSeconds = 0.1f;
		if (DamageIntervalMs < 50) DamageIntervalMs = 50;
		if (DamageFraction <= 0f) DamageFraction = 0.005f;
		if (DamageSoundVolume < 0f) DamageSoundVolume = 0f;
		if (DamageSoundVolume > 1f) DamageSoundVolume = 1f;
	}
}

public class ScourgePlugin : DeadworksPluginBase {
	public override string Name => "Scourge DOT";

	[PluginConfig]
	public ScourgeConfig Config { get; set; } = new();

	private static readonly EntityData<IHandle> _dotTimers = new();

	public override void OnLoad(bool isReload) => Console.WriteLine(isReload ? "Scourge reloaded!" : "Scourge loaded!");
	public override void OnUnload() {
		Console.WriteLine("Scourge unloaded!");
		_dotTimers.Clear();
	}

	public override HookResult OnTakeDamage(TakeDamageEvent args) {
		if (args.Info.Ability?.SubclassVData?.Name != "upgrade_discord")
			return HookResult.Continue;
		var pawn = args.Entity.As<CCitadelPlayerPawn>();
		if (pawn == null)
			return HookResult.Continue;

		var attacker = args.Info.Attacker;
		uint victimHandle = pawn.EntityHandle;

		if (_dotTimers.TryGet(pawn, out var existing))
			existing.Cancel();

		int maxTicks = (int)(Config.DurationSeconds * 1000 / Config.DamageIntervalMs);
		float damageFraction = Config.DamageFraction;
		string sound = Config.DamageSound;
		float volume = Config.DamageSoundVolume;
		int intervalMs = Config.DamageIntervalMs;

		var handle = Timer.Sequence(step => {
			if (step.Run > maxTicks)
				return step.Done();

			var ent = CBaseEntity.FromHandle(victimHandle);
			if (ent == null || !ent.IsAlive)
				return step.Done();

			var healthMax = pawn.Controller?.PlayerDataGlobal.HealthMax ?? 0;
			if (healthMax <= 0)
				return step.Done();

			ent.Hurt(healthMax * damageFraction, attacker: attacker);
			ent.EmitSound(sound, volume: volume);

			return step.Wait(intervalMs.Milliseconds());
		});

		_dotTimers[pawn] = handle;
		return HookResult.Continue;
	}
}
