namespace DeadworksManaged.Api;

/// <summary>Helpers for sending chat messages to players.</summary>
public static class Chat
{
	/// <summary>Sends a chat message to a single player by slot index.</summary>
	public static void PrintToChat(int slot, string text)
	{
		var msg = new CCitadelUserMsg_ChatMsg
		{
			PlayerSlot = slot,
			Text = text,
			AllChat = true,
		};
		NetMessages.Send(msg, RecipientFilter.Single(slot));
	}

	/// <summary>Sends a chat message to a single player via their controller.</summary>
	public static void PrintToChat(CCitadelPlayerController controller, string text)
	{
		PrintToChat(controller.EntityIndex - 1, text);
	}

	/// <summary>Sends a chat message to all connected players.</summary>
	public static void PrintToChatAll(string text)
	{
		foreach (var controller in Players.GetAll())
		{
			int slot = controller.EntityIndex - 1;
			PrintToChat(slot, text);
		}
	}
}
