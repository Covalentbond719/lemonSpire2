using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;

namespace lemonSpire2.PlayerColor;

/// <summary>
///     玩家颜色选择按钮 Patch
///     在本地玩家的状态栏添加一个颜色选择按钮，点击后调用系统原生颜色选择器
/// </summary>
[HarmonyPatchCategory("PlayerColor")]
[HarmonyPatch(typeof(NMultiplayerPlayerState))]
public static class PlayerColorButtonPatch
{
    private const string ColorPickerEmoji = "🎨";

    private static readonly FieldInfo? TopContainerField =
        typeof(NMultiplayerPlayerState).GetField("_topContainer", BindingFlags.NonPublic | BindingFlags.Instance);

    [HarmonyPostfix]
    [HarmonyPatch("_Ready")]
    public static void ReadyPostfix(NMultiplayerPlayerState __instance)
    {
        // 只对本地玩家显示颜色按钮
        if (!LocalContext.IsMe(__instance.Player)) return;

        var topContainer = TopContainerField?.GetValue(__instance) as HBoxContainer;
        if (topContainer == null)
        {
            ColorManager.Log.Warn("Failed to get TopContainer from NMultiplayerPlayerState");
            return;
        }

        // 创建颜色选择按钮
        var colorButton = CreateColorButton(__instance);
        topContainer.AddChild(colorButton);
    }

    private static Button CreateColorButton(NMultiplayerPlayerState playerState)
    {
        var button = new Button
        {
            CustomMinimumSize = new Vector2(28, 28),
            TooltipText = "选择玩家颜色"
        };

        // 添加 emoji 标签
        var emojiLabel = new Label
        {
            Text = ColorPickerEmoji,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        emojiLabel.AddThemeFontSizeOverride("font_size", 18);
        emojiLabel.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        button.AddChild(emojiLabel);

        // 设置透明背景样式
        var transparentStyle = new StyleBoxFlat
        {
            BgColor = new Color(1, 1, 1, 0)
        };
        button.AddThemeStyleboxOverride("normal", transparentStyle);
        button.AddThemeStyleboxOverride("hover", transparentStyle);
        button.AddThemeStyleboxOverride("pressed", transparentStyle);

        // 连接点击事件
        button.Pressed += () => OnColorButtonPressed(playerState);

        return button;
    }

    private static void OnColorButtonPressed(NMultiplayerPlayerState playerState)
    {
        // 获取当前颜色作为初始值
        var currentColor = ColorManager.Instance.GetCustomColor(playerState.Player.NetId) ?? Colors.White;

        // 调用系统原生颜色选择器
        DisplayServer.ColorPicker(Callable.From<Color>(color => { OnColorPicked(playerState.Player.NetId, color); }));
    }

    private static void OnColorPicked(ulong playerId, Color color)
    {
        // 设置本地颜色
        ColorManager.Instance.SetPlayerColor(playerId, color);

        // 广播给其他玩家
        ColorNetworkPatch.NetworkHandler?.BroadcastColorChange(playerId, color);

        ColorManager.Log.Info($"Player {playerId} changed color to {color}");
    }
}
