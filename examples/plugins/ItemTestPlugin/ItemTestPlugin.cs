using DeadworksManaged.Api;

namespace ItemTestPlugin;

public class ItemTestPlugin : DeadworksPluginBase
{
	public override string Name => "Item Test";

	public override void OnLoad(bool isReload) { }
	public override void OnUnload() { }

	[Command("additem", Description = "Give an item directly (no cost). Set enhanced=true for the upgraded version.")]
	public void CmdAddItem(CCitadelPlayerController caller, string itemName, bool enhanced = false)
	{
		var pawn = caller.GetHeroPawn();
		if (pawn == null) return;

		var item = pawn.AddItem(itemName, enhanced);
		Reply(caller, item != null
			? $"Added '{itemName}' (enhanced={enhanced}) -> entity #{item.EntityIndex}"
			: $"Failed to add '{itemName}' (enhanced={enhanced})");
	}

	[Command("sellitem", Description = "Sell an item. fullRefund=true refunds the full price.")]
	public void CmdSellItem(CCitadelPlayerController caller, string itemName, bool fullRefund = false)
	{
		var pawn = caller.GetHeroPawn();
		if (pawn == null) return;

		bool ok = pawn.SellItem(itemName, fullRefund);
		Reply(caller, ok
			? $"Sold '{itemName}' (fullRefund={fullRefund})"
			: $"Failed to sell '{itemName}'");
	}

	[Command("removeitem", Description = "Remove an item from your inventory (no refund)")]
	public void CmdRemoveItem(CCitadelPlayerController caller, string itemName)
	{
		var pawn = caller.GetHeroPawn();
		if (pawn == null) return;

		bool ok = pawn.RemoveItem(itemName);
		Reply(caller, ok
			? $"Removed '{itemName}'"
			: $"Failed to remove '{itemName}'");
	}

	[Command("givegold", Description = "Give yourself gold (default 50000)")]
	public void CmdGiveGold(CCitadelPlayerController caller, int amount = 50000)
	{
		var pawn = caller.GetHeroPawn();
		if (pawn == null) return;

		pawn.ModifyCurrency(ECurrencyType.EGold, amount, ECurrencySource.ECheats, silent: true, forceGain: true);
		Reply(caller, $"Gave {amount} gold");
	}

	[Command("listitems", Description = "List all abilities/items currently on your pawn")]
	public void CmdListItems(CCitadelPlayerController caller)
	{
		var pawn = caller.GetHeroPawn();
		if (pawn == null) return;

		var abilities = pawn.AbilityComponent.Abilities;
		Reply(caller, $"Pawn has {abilities.Count} abilities/items:");
		foreach (var ent in abilities)
		{
			Reply(caller, $"  #{ent.EntityIndex}: {ent.DesignerName} ({ent.Classname})");
		}
	}

	[Command("rcon", Description = "Execute a server console command", SuppressChat = true)]
	public void CmdRcon(CCitadelPlayerController? caller, params string[] commandParts)
	{
		if (commandParts.Length == 0)
			throw new CommandException("Nothing to execute.");

		string command = string.Join(' ', commandParts);
		Server.ExecuteCommand(command);
		Reply(caller, $"Executed: {command}");
	}

	private static void Reply(CCitadelPlayerController? to, string message)
	{
		if (to != null) to.PrintToConsole(message);
		else Console.WriteLine(message);
	}
}
