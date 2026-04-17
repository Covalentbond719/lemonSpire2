using lemonSpire2.util.Net;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Game;

namespace lemonSpire2.PlayerStateEx.RemoteFlash;

public sealed class RemoteUiFlashNetworkHandler : NetworkHandlerBase<RemoteUiFlashMessage>
{
    public RemoteUiFlashNetworkHandler(INetGameService netService) : base(netService)
    {
        Log.Info("RemoteUiFlashNetworkHandler initialized");
    }

    internal static Logger Log { get; } = new("lemon.player.flash", LogType.Network);

    public void Send(RemoteUiFlashMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);
        SendMessage(message);
        OnReceiveMessage(message, LocalPlayerId);
    }

    protected override void OnReceiveMessage(RemoteUiFlashMessage message, ulong senderId)
    {
        ArgumentNullException.ThrowIfNull(message);
        if (message.TargetPlayerId != LocalPlayerId) return;

        var target = RemoteUiFlashResolver.FindVisibleTarget(message);
        if (target == null)
        {
            Log.Debug($"Remote flash target not found for kind={message.Kind}");
            return;
        }

        RemoteUiFlashPlayer.FlashGreen(target);
        Log.Debug($"Applied remote flash for kind={message.Kind} from sender={senderId}");
    }
}
