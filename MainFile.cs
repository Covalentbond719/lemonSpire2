using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using lemonSpire2.Chat;
using lemonSpire2.PlayerTooltip;
using lemonSpire2.StatsTracker;
using lemonSpire2.SynergyIndicator;

namespace lemonSpire2;

[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    internal const string ModId = "lemonSpire2";

    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } =
        new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);

    #region Feature Flags

    /// <summary>聊天系统 (多人游戏)</summary>
    public static bool EnableChat { get; set; } = true;
    

    /// <summary>握手指示器</summary>
    public static bool EnableHandshakeIndicator { get; set; } = true;

    /// <summary>统计追踪器</summary>
    public static bool EnableStatsTracker { get; set; } = true;

    #endregion

    public static void Initialize()
    {
        Harmony harmony = new(ModId);

        if (EnableChat)
            harmony.CreateClassProcessor(typeof(ChatUIPatch)).Patch();

        if (EnableHandshakeIndicator)
            harmony.CreateClassProcessor(typeof(SynergyIndicatorPatch)).Patch();

        if (EnableStatsTracker)
        {
            StatsTrackerManager.Instance.Initialize();
            PlayerTooltipRegistry.Register(new StatsTooltipProvider());
        }

        if (PlayerTooltipRegistry.HasProviders)
            harmony.CreateClassProcessor(typeof(NMultiplayerPlayerStatePatch)).Patch();

        Logger.Info("lemonSpire2 mod initialized");
    }
}