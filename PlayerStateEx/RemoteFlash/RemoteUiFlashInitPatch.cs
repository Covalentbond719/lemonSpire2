using HarmonyLib;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Runs;

namespace lemonSpire2.PlayerStateEx.RemoteFlash;

[HarmonyPatchCategory("PlayerRemoteFlash")]
[HarmonyPatch(typeof(NGlobalUi), "Initialize")]
public static class RemoteUiFlashInitPatch
{
    [HarmonyPostfix]
    public static void Postfix()
    {
        var netService = RunManager.Instance.NetService;
        RemoteUiFlashSynchronizer.Reset();

        if (!netService.Type.IsMultiplayer())
        {
            RemoteUiFlashNetworkHandler.Log.Debug("Not multiplayer, skipping remote flash init");
            return;
        }

        RemoteUiFlashSynchronizer.Initialize(netService);
    }
}
