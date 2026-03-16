using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;

namespace lemonSpire2.PlayerColor;

/// <summary>
///     远程鼠标颜色 Patch
///     修改远程玩家鼠标的颜色
/// </summary>
[HarmonyPatchCategory("PlayerColor")]
[HarmonyPatch(typeof(NRemoteMouseCursor))]
public static class RemoteCursorColorPatch
{
    /// <summary>
    ///     混合模式配置
    ///     <para>CanvasItemMaterial.BlendModeEnum 选项说明：</para>
    ///     <para>- Mix: 默认混合，颜色与背景按透明度混合</para>
    ///     <para>- Add: 加法混合，颜色叠加到背景上，暗色也会变亮</para>
    ///     <para>- Sub: 减法混合，从背景减去颜色</para>
    ///     <para>- Mul: 乘法混合，颜色与背景相乘</para>
    ///     <para>- PremultAlpha: 预乘 Alpha</para>
    /// </summary>
    private const CanvasItemMaterial.BlendModeEnum CursorBlendMode = CanvasItemMaterial.BlendModeEnum.Add;

    private static readonly List<(WeakReference<NRemoteMouseCursor> Cursor, Action<ulong, Color> Handler)>
        Registrations = new();

    [HarmonyPostfix]
    [HarmonyPatch("_Ready")]
    public static void ReadyPostfix(NRemoteMouseCursor __instance)
    {
        var playerId = __instance.PlayerId;

        // 创建颜色变更回调
        Action<ulong, Color> handler = (changedPlayerId, color) =>
        {
            if (changedPlayerId == playerId) UpdateCursorColor(__instance, color);
        };

        // 使用弱引用存储，避免内存泄漏
        CleanupDeadReferences();
        Registrations.Add((new WeakReference<NRemoteMouseCursor>(__instance), handler));
        ColorManager.Instance.OnPlayerColorChanged += handler;

        // 设置初始颜色
        var customColor = ColorManager.Instance.GetCustomColor(playerId);
        if (customColor.HasValue) UpdateCursorColor(__instance, customColor.Value);
    }

    private static void CleanupDeadReferences()
    {
        for (var i = Registrations.Count - 1; i >= 0; i--)
        {
            if (Registrations[i].Cursor.TryGetTarget(out var cursor) && GodotObject.IsInstanceValid(cursor)) continue;
            ColorManager.Instance.OnPlayerColorChanged -= Registrations[i].Handler;
            Registrations.RemoveAt(i);
        }
    }

    private static void UpdateCursorColor(NRemoteMouseCursor instance, Color color)
    {
        var textureRect = instance.GetNode<TextureRect>("TextureRect");
        if (textureRect == null) return;

        textureRect.Modulate = color;
        textureRect.Material = new CanvasItemMaterial
        {
            BlendMode = CursorBlendMode
        };
    }
}
