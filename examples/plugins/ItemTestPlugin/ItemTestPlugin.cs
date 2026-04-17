using DeadworksManaged.Api;

namespace ItemTestPlugin;

public class ItemTestPlugin : DeadworksPluginBase
{
	public override string Name => "Item Test";

	public override void OnLoad(bool isReload) { }
	public override void OnUnload() { }

	/// <summary>
	/// !additem &lt;item_name&gt; [tier]
	/// Gives an item directly (no cost). Optional tier for upgraded versions (0-based).
	/// Example: !additem upgrade_sprint_booster 1
	/// </summary>
	[ChatCommand("additem")]
	public HookResult CmdAddItem(ChatCommandContext ctx)
	{
		var pawn = ctx.Controller?.GetHeroPawn();
		if (pawn == null) return HookResult.Handled;

		if (ctx.Args.Length < 1)
		{
			Reply(ctx, "Usage: !additem <item_name> [tier]");
			return HookResult.Handled;
		}

		string itemName = ctx.Args[0];
		int tier = ctx.Args.Length >= 2 && int.TryParse(ctx.Args[1], out var t) ? t : -1;

		var item = pawn.AddItem(itemName, tier);
		Reply(ctx, item != null
			? $"Added '{itemName}' (tier {tier}) -> entity #{item.EntityIndex}"
			: $"Failed to add '{itemName}' (tier {tier})");

		return HookResult.Handled;
	}

	/// <summary>
	/// !sellitem &lt;item_name&gt; [fullRefund: 0/1]
	/// Sells an item with gold refund.
	/// Example: !sellitem upgrade_sprint_booster 1
	/// </summary>
	[ChatCommand("sellitem")]
	public HookResult CmdSellItem(ChatCommandContext ctx)
	{
		var pawn = ctx.Controller?.GetHeroPawn();
		if (pawn == null) return HookResult.Handled;

		if (ctx.Args.Length < 1)
		{
			Reply(ctx, "Usage: !sellitem <item_name> [fullRefund: 0/1]");
			return HookResult.Handled;
		}

		string itemName = ctx.Args[0];
		bool fullRefund = ctx.Args.Length >= 2 && ctx.Args[1] == "1";

		bool ok = pawn.SellItem(itemName, fullRefund);
		Reply(ctx, ok
			? $"Sold '{itemName}' (fullRefund={fullRefund})"
			: $"Failed to sell '{itemName}'");

		return HookResult.Handled;
	}

	/// <summary>
	/// !removeitem &lt;item_name&gt;
	/// Removes an item directly (no refund).
	/// Example: !removeitem upgrade_sprint_booster
	/// </summary>
	[ChatCommand("removeitem")]
	public HookResult CmdRemoveItem(ChatCommandContext ctx)
	{
		var pawn = ctx.Controller?.GetHeroPawn();
		if (pawn == null) return HookResult.Handled;

		if (ctx.Args.Length < 1)
		{
			Reply(ctx, "Usage: !removeitem <item_name>");
			return HookResult.Handled;
		}

		string itemName = ctx.Args[0];
		bool ok = pawn.RemoveItem(itemName);
		Reply(ctx, ok
			? $"Removed '{itemName}'"
			: $"Failed to remove '{itemName}'");

		return HookResult.Handled;
	}

	/// <summary>
	/// !givegold [amount]
	/// Gives gold to the player (default 50000).
	/// Example: !givegold 10000
	/// </summary>
	[ChatCommand("givegold")]
	public HookResult CmdGiveGold(ChatCommandContext ctx)
	{
		var pawn = ctx.Controller?.GetHeroPawn();
		if (pawn == null) return HookResult.Handled;

		int amount = 50000;
		if (ctx.Args.Length >= 1 && int.TryParse(ctx.Args[0], out var a))
			amount = a;

		pawn.ModifyCurrency(ECurrencyType.EGold, amount, ECurrencySource.ECheats, silent: true, forceGain: true);
		Reply(ctx, $"Gave {amount} gold");

		return HookResult.Handled;
	}

	/// <summary>
	/// !listitems
	/// Lists all abilities/items currently on the pawn.
	/// </summary>
	[ChatCommand("listitems")]
	public HookResult CmdListItems(ChatCommandContext ctx)
	{
		var pawn = ctx.Controller?.GetHeroPawn();
		if (pawn == null) return HookResult.Handled;

		var abilities = pawn.AbilityComponent.Abilities;
		Reply(ctx, $"Pawn has {abilities.Count} abilities/items:");
		foreach (var ent in abilities)
		{
			var designer = ent.DesignerName;
			var classname = ent.Classname;
			Reply(ctx, $"  #{ent.EntityIndex}: {designer} ({classname})");
		}

		return HookResult.Handled;
	}

	/// <summary>
	/// !rcon &lt;command&gt;
	/// Executes a server console command.
	/// Example: !rcon sv_cheats 1
	/// </summary>
	[ChatCommand("rcon")]
	public HookResult CmdRcon(ChatCommandContext ctx)
	{
		if (ctx.Args.Length < 1)
		{
			Reply(ctx, "Usage: !rcon <command>");
			return HookResult.Handled;
		}

		string command = string.Join(' ', ctx.Args);
		Server.ExecuteCommand(command);
		Reply(ctx, $"Executed: {command}");

		return HookResult.Handled;
	}

	private static void Reply(ChatCommandContext ctx, string message)
	{
		ctx.Controller?.PrintToConsole(message);
	}
}
