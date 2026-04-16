using lemonSpire2.Chat;
using lemonSpire2.Chat.Message;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;

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
}
