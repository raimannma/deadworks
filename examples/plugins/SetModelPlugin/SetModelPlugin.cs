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

	[ChatCommand("werewolf")]
	public HookResult CmdWerewolf(ChatCommandContext ctx)
	{
		var pawn = ctx.Controller?.GetHeroPawn();
		if (pawn == null) return HookResult.Handled;

		pawn.SetModel(WerewolfModel);

		var msg = new CCitadelUserMsg_HudGameAnnouncement
		{
			TitleLocstring = "MODEL SWAP",
			DescriptionLocstring = "You are now a werewolf!"
		};
		NetMessages.Send(msg, RecipientFilter.Single(ctx.Message.SenderSlot));

		return HookResult.Handled;
	}
}
