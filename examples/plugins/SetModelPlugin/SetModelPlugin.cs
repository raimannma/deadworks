using DeadworksManaged.Api;

namespace SetModelPlugin;

public class SetModelPlugin : DeadworksPluginBase
{
	public override string Name => "SetModel Example";

	public override void OnLoad(bool isReload) { }
	public override void OnUnload() { }

	private const string WerewolfModel = "models/heroes_wip/werewolf/werewolf.vmdl";

	public override void OnPrecacheResources()
	{
		Precache.AddResource(WerewolfModel);
	}

	[Command("werewolf", Description = "Turn yourself into a werewolf")]
	public void CmdWerewolf(CCitadelPlayerController caller)
	{
		var pawn = caller.GetHeroPawn();
		if (pawn == null) return;

		pawn.SetModel(WerewolfModel);

		var msg = new CCitadelUserMsg_HudGameAnnouncement
		{
			TitleLocstring = "MODEL SWAP",
			DescriptionLocstring = "You are now a werewolf!"
		};
		NetMessages.Send(msg, RecipientFilter.Single(caller.EntityIndex - 1));
	}
}
