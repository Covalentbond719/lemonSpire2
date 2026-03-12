using Godot;
using lemonSpire2.Chat.Intent;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Potions;

namespace lemonSpire2.NPlayerState.Panel;

/// <summary>
///     药水显示提供者
///     显示玩家的药水栏
///     支持 Alt+Click 发送药水到聊天
/// </summary>
public class PotionProvider : IPlayerPanelProvider
{
    private const float PotionScale = 0.6f;

    public string ProviderId => "potions";
    public int Priority => 20;
    public string DisplayName => "Potions";

    public bool ShouldShow(Player player)
    {
        // 只显示有药水的玩家
        return player.Potions.Any();
    }

    public Control CreateContent(Player player)
    {
        var container = new HBoxContainer
        {
            Name = "PotionsContainer",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        container.AddThemeConstantOverride("separation", 4);

        // 不在这里调用 UpdateContent，等待加入场景树后再调用
        return container;
    }

    public void UpdateContent(Player player, Control content)
    {
        if (content is not HBoxContainer container) return;

        // 清除现有内容
        foreach (var child in container.GetChildren())
        {
            child.QueueFree();
        }

        MainFile.Logger.Info($"[PotionProvider] Updating content, player has {player.Potions.Count()} potions");

        foreach (var potion in player.Potions)
        {
            MainFile.Logger.Info($"[PotionProvider] Creating NPotion for {potion.Id.Entry}");
            
            var nPotion = NPotion.Create(potion);
            if (nPotion == null)
            {
                MainFile.Logger.Warn($"[PotionProvider] NPotion.Create returned null for {potion.Id.Entry}");
                continue;
            }

            var holder = NPotionHolder.Create(isUsable: false);
            if (holder == null)
            {
                MainFile.Logger.Warn($"[PotionProvider] NPotionHolder.Create returned null");
                nPotion.QueueFree();
                continue;
            }

            holder.Scale = new Vector2(PotionScale, PotionScale);
            container.AddChild(holder);
            holder.AddPotion(nPotion);
            nPotion.Position = Vector2.Zero;  // 关键：重置位置，否则会出现偏移

            MainFile.Logger.Info($"[PotionProvider] Added potion {potion.Id.Entry} to holder");
        }
    }

    public Action? SubscribeEvents(Player player, Action onUpdate)
    {
        MainFile.Logger.Debug($"[PotionProvider] SubscribeEvents for player with {player.Potions.Count()} potions");
        
        // 订阅药水变化事件
        // 事件类型是 Action<PotionModel>，需要适配
        void OnPotionChanged(PotionModel potion)
        {
            MainFile.Logger.Debug($"[PotionProvider] Potion event triggered for {potion?.Id.Entry ?? "null"}");
            onUpdate();
        }

        player.PotionProcured += OnPotionChanged;
        player.PotionDiscarded += OnPotionChanged;
        player.UsedPotionRemoved += OnPotionChanged;

        return () =>
        {
            player.PotionProcured -= OnPotionChanged;
            player.PotionDiscarded -= OnPotionChanged;
            player.UsedPotionRemoved -= OnPotionChanged;
        };
    }

    public void Cleanup(Control content)
    {
        foreach (var child in content.GetChildren())
        {
            if (child is Node node)
            {
                node.QueueFree();
            }
        }
    }
}
