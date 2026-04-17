using lemonSpire2.Chat;
using lemonSpire2.Chat.Message;
using lemonSpire2.PlayerStateEx.RemoteFlash;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace lemonSpire2.PlayerStateEx.PanelProvider;

internal static class PlayerPanelChatHelper
{
    public static void SendPlayerItemToChat(Player player, string locEntryKey, TooltipSegment tooltipSegment)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentException.ThrowIfNullOrWhiteSpace(locEntryKey);
        ArgumentNullException.ThrowIfNull(tooltipSegment);

        ChatStore.SendToChat(
            new TemplateSegment
            {
                Template = new LocString("gameplay_ui", locEntryKey),
                Slots =
                [
                    EntitySegment.FromPlayer(player).ToNamedSegment("Player"),
                    tooltipSegment.ToNamedSegment("Item")
                ]
            }
        );
    }

    public static void RequestRemoteFlash(Player player, RemoteUiFlashKind kind, CardModel card)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(card);

        var snapshotSource = card.IsMutable ? card : (CardModel)card.MutableClone();
        RemoteUiFlashSynchronizer.Send(new RemoteUiFlashMessage
        {
            SenderId = RunManager.Instance.NetService.NetId,
            TargetPlayerId = player.NetId,
            Kind = kind,
            Card = snapshotSource.ToSerializable()
        });
    }

    public static void RequestRemoteFlash(Player player, RemoteUiFlashKind kind, PotionModel potion)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(potion);

        var slotIndex = player.GetPotionSlotIndex(potion);
        if (slotIndex < 0 && potion.Owner != null)
            slotIndex = potion.Owner.GetPotionSlotIndex(potion);

        var snapshot = new SerializablePotion
        {
            Id = potion.Id,
            SlotIndex = Math.Max(0, slotIndex)
        };

        RemoteUiFlashSynchronizer.Send(new RemoteUiFlashMessage
        {
            SenderId = RunManager.Instance.NetService.NetId,
            TargetPlayerId = player.NetId,
            Kind = kind,
            Potion = snapshot
        });
    }

    public static void RequestRemoteFlash(Player player, RemoteUiFlashKind kind, RelicModel relic)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(relic);

        var snapshotSource = relic.IsMutable ? relic : (RelicModel)relic.MutableClone();
        RemoteUiFlashSynchronizer.Send(new RemoteUiFlashMessage
        {
            SenderId = RunManager.Instance.NetService.NetId,
            TargetPlayerId = player.NetId,
            Kind = kind,
            Relic = snapshotSource.ToSerializable()
        });
    }
}
