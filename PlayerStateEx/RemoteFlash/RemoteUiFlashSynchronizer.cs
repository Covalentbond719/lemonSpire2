using MegaCrit.Sts2.Core.Multiplayer.Game;

namespace lemonSpire2.PlayerStateEx.RemoteFlash;

public static class RemoteUiFlashSynchronizer
{
    private static RemoteUiFlashNetworkHandler? _handler;

    public static void Initialize(INetGameService netService)
    {
        ArgumentNullException.ThrowIfNull(netService);
        _handler?.Dispose();
        _handler = new RemoteUiFlashNetworkHandler(netService);
    }

    public static void Reset()
    {
        _handler?.Dispose();
        _handler = null;
    }

    public static void Send(RemoteUiFlashMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);
        _handler?.Send(message);
    }
}
