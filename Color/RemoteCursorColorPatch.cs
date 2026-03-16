using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;

namespace lemonSpire2.PlayerColor;

/// <summary>
///     远程鼠标颜色 Patch
///     修改远程玩家鼠标的颜色，使用 Shader 实现去色 + 着色效果
/// </summary>
[HarmonyPatchCategory("PlayerColor")]
[HarmonyPatch(typeof(NRemoteMouseCursor))]
public static class RemoteCursorColorPatch
{
    /// <summary>
    ///     基础亮度（提高鼠标可见度）
    /// </summary>
    private const float BaseBrightness = 1.5f;

    /// <summary>
    ///     基础透明度下限
    /// </summary>
    private const float MinAlpha = 0.9f;

    /// <summary>
    ///     去色 + 着色 Shader（静态，只创建一次）
    /// </summary>
    private static Shader? _desaturateShader;

    private static readonly List<(WeakReference<NRemoteMouseCursor> Cursor, Action<ulong, Color> Handler)>
        Registrations = new();

    /// <summary>
    ///     去色着色 Shader 代码
    ///     1. 将纹理转换为灰度（去色）
    ///     2. 提高亮度
    ///     3. 应用玩家颜色（着色）
    /// </summary>
    private static readonly string DesaturateShaderCode = @"
shader_type canvas_item;
render_mode blend_add;

uniform vec4 tint_color : source_color = vec4(1.0, 1.0, 1.0, 1.0);
uniform float brightness : hint_range(0.5, 2.5) = 1.5;

void fragment() {
    vec4 tex_color = texture(TEXTURE, UV);

    // 去色：转换为灰度（使用标准亮度权重）
    float gray = dot(tex_color.rgb, vec3(0.299, 0.587, 0.114));

    // 提高亮度
    gray *= brightness;

    // 应用玩家颜色（灰度 × 颜色 = 着色效果）
    COLOR = vec4(gray * tint_color.rgb, tex_color.a * tint_color.a);
}
";

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

    private static void UpdateCursorColor(NRemoteMouseCursor instance, Color playerColor)
    {
        var textureRect = instance.GetNode<TextureRect>("TextureRect");
        if (textureRect == null) return;

        // 确保 Shader 已创建
        _desaturateShader ??= CreateDesaturateShader();

        // 创建新的 ShaderMaterial（每个 TextureRect 独立，以便设置不同颜色）
        var material = new ShaderMaterial
        {
            Shader = _desaturateShader
        };

        // 设置着色参数
        material.SetShaderParameter("tint_color", playerColor);
        material.SetShaderParameter("brightness", BaseBrightness);

        textureRect.Material = material;

        // 重置 Modulate（Shader 会处理颜色）
        textureRect.Modulate = Colors.White;
        textureRect.SelfModulate = new Color(1, 1, 1, MinAlpha);
    }

    private static Shader CreateDesaturateShader()
    {
        var shader = new Shader
        {
            Code = DesaturateShaderCode
        };
        return shader;
    }
}
