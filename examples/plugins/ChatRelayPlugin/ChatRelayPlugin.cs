using DeadworksManaged.Api;

namespace ChatRelayPlugin;

public class ChatRelayPlugin : DeadworksPluginBase {
	public override string Name => "ChatRelay";

	private bool _rebroadcasting;

	public override void OnLoad(bool isReload) {
		Console.WriteLine(isReload ? "[ChatRelay] Reloaded!" : "[ChatRelay] Loaded!");
	}

	public override void OnUnload() {
		Console.WriteLine("[ChatRelay] Unloaded!");
	}

	[NetMessageHandler]
	public HookResult OnChatMsgOutgoing(OutgoingMessageContext<CCitadelUserMsg_ChatMsg> ctx) {
		if (_rebroadcasting) return HookResult.Continue;

		var senderSlot = ctx.Message.PlayerSlot;
		if (senderSlot < 0) return HookResult.Continue;

		if (senderSlot < 12)
			return HookResult.Continue;

		var text = ctx.Message.Text;
		var allChat = ctx.Message.AllChat;
		var laneColor = ctx.Message.LaneColor;
		var originalMask = ctx.Recipients.Mask;

		var senderController = CBaseEntity.FromIndex<CCitadelPlayerController>(senderSlot + 1);
		var senderName = senderController?.PlayerName ?? $"Player {senderSlot}";

		_rebroadcasting = true;
		try {
			for (int slot = 0; slot < 64; slot++) {
				if ((originalMask & (1UL << slot)) == 0) continue;
				var msg = new CCitadelUserMsg_ChatMsg {
					PlayerSlot = slot,
					Text = slot == senderSlot ? text : $"[{senderName}]: {text}",
					AllChat = allChat,
					LaneColor = laneColor
				};
				NetMessages.Send(msg, RecipientFilter.Single(slot));
			}
		} finally {
			_rebroadcasting = false;
		}

		return HookResult.Stop;
	}
}
