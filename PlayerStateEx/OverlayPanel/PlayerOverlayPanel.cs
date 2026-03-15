using Godot;
using lemonSpire2.PlayerStateEx.PanelProvider;
using lemonSpire2.SyncShop;
using lemonSpire2.util.Ui;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Runs;
using DraggableTitleBar = lemonSpire2.util.Ui.DraggableTitleBar;
using ViewportResizeNotifier = lemonSpire2.util.Ui.ViewportResizeNotifier;

using Logger = MegaCrit.Sts2.Core.Logging.Logger;

namespace lemonSpire2.PlayerStateEx.OverlayPanel;

/// <summary>
///     玩家悬浮面板
///     显示其他玩家的手牌、药水等信息
///     支持拖拽、Alt+Click 发送物品
/// </summary>
public partial class PlayerOverlayPanel : Control
{
    private static Logger Log => PlayerPanelRegistry.Log;
    private const float MinContentHeight = 80f;
    private const float PanelWidth = 400f;
    private readonly Dictionary<string, Control> _providerContents = [];
    private readonly List<Action> _unsubscribeActions = [];

    private VBoxContainer? _contentContainer;
    private DraggableTitleBar? _header;
    private Label? _headerTitle;
    private VBoxContainer? _mainContainer;
    private PanelContainer? _panel;
    private Player? _player;
    private ScrollContainer? _scrollContainer;

    private bool _needsRefresh;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Stop;
        CreateUi();
        ViewportResizeNotifier.Instance.OnViewportResized += _ => PanelPositionHelper.ClampToViewport(_panel);
    }

    public override void _Process(double delta)
    {
        if (_needsRefresh)
        {
            _needsRefresh = false;
            if (_player != null) RefreshAllProviders();
        }

        // 每帧更新面板尺寸
        UpdatePanelSize();
    }

    private void CreateUi()
    {
        _panel = new PanelContainer
        {
            Name = "Panel",
            AnchorsPreset = (int)LayoutPreset.TopLeft
        };

        var style = new StyleBoxFlat
        {
            BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f),
            BorderColor = new Color(0.4f, 0.4f, 0.5f),
            CornerRadiusTopLeft = 4,
            CornerRadiusTopRight = 4,
            CornerRadiusBottomLeft = 4,
            CornerRadiusBottomRight = 4,
            ContentMarginLeft = 4,
            ContentMarginRight = 4,
            ContentMarginTop = 4,
            ContentMarginBottom = 4
        };
        _panel.AddThemeStyleboxOverride("panel", style);
        AddChild(_panel);

        _mainContainer = new VBoxContainer
        {
            Name = "MainContainer",
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        _panel.AddChild(_mainContainer);

        _header = new DraggableTitleBar { Name = "Header" };
        _header.SetDragTarget(_panel);
        _header.SetDragCallbacks(onDragEnd: () => PanelPositionHelper.ClampToViewport(_panel));
        _headerTitle = _header.GetTitleLabel();
        _header.ShowCloseButton(OnCloseButtonPressed);
        _mainContainer.AddChild(_header);

        var separator = new HSeparator { Name = "Separator" };
        separator.AddThemeColorOverride("separator_color", new Color(0.3f, 0.3f, 0.4f));
        _mainContainer.AddChild(separator);

        _scrollContainer = new ScrollContainer
        {
            Name = "ScrollContainer",
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
            VerticalScrollMode = ScrollContainer.ScrollMode.Auto
        };
        _mainContainer.AddChild(_scrollContainer);

        _contentContainer = new VBoxContainer
        {
            Name = "ContentContainer",
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        _contentContainer.AddThemeConstantOverride("separation", 8);
        _scrollContainer.AddChild(_contentContainer);
    }

    /// <summary>
    ///     每帧更新面板尺寸
    /// </summary>
    private void UpdatePanelSize()
    {
        if (_scrollContainer == null || _contentContainer == null || _panel == null || _mainContainer == null)
            return;

        var contentHeight = _contentContainer.GetMinimumSize().Y;
        var currentHeight = _scrollContainer.Size.Y;
        var maxHeight = GetMaxContentHeight();
        var targetHeight = Mathf.Clamp(contentHeight, MinContentHeight, maxHeight);

        // 如果内容变少了，需要重置让容器收缩
        if (contentHeight < currentHeight)
        {
            _scrollContainer.Size = new Vector2(PanelWidth, 0);
        }

        _scrollContainer.CustomMinimumSize = new Vector2(0, targetHeight);
    }

    private float GetMaxContentHeight()
    {
        var viewportHeight = GetViewport()?.GetVisibleRect().Size.Y ?? 1080f;
        return viewportHeight * 0.5f;
    }

    public void Initialize(Player player)
    {
        _player = player ?? throw new ArgumentNullException(nameof(player));
        _headerTitle!.Text = PlatformUtil.GetPlayerName(RunManager.Instance.NetService.Platform, player.NetId);

        PlayerPanelRegistry.Initialize();

        CombatManager.Instance.CombatSetUp += OnCombatSetUp;

        RefreshAllProviders();
    }

    private void RefreshAllProviders()
    {
        if (_player == null || _contentContainer == null) return;

        ClearProviderContents();
        CreateProviderContents();
        PanelPositionHelper.ClampToViewport(_panel);
    }

    private void CreateProviderContents()
    {
        if (_player == null || _contentContainer == null) return;

        foreach (var provider in PlayerPanelRegistry.GetProviders())
        {
            if (!provider.ShouldShow(_player)) continue;

            // 创建区块容器
            var sectionContainer = new VBoxContainer
            {
                Name = $"Section_{provider.ProviderId}",
                SizeFlagsHorizontal = SizeFlags.ExpandFill
            };
            sectionContainer.AddThemeConstantOverride("separation", 4);

            // 区块标题
            var sectionTitle = new Label
            {
                Text = provider.DisplayName,
                MouseFilter = MouseFilterEnum.Ignore
            };
            sectionTitle.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.7f));
            sectionTitle.AddThemeFontSizeOverride("font_size", 20);
            sectionContainer.AddChild(sectionTitle);

            _contentContainer.AddChild(sectionContainer);

            var content = provider.CreateContent(_player);
            _providerContents[provider.ProviderId] = content;
            sectionContainer.AddChild(content);

            provider.UpdateContent(_player, content);

            var unsubscribe = provider.SubscribeEvents(_player, () =>
            {
                if (_providerContents.TryGetValue(provider.ProviderId, out var c))
                    provider.UpdateContent(_player, c);
                // 不需要手动触发高度更新，_Process 会每帧处理
            });
            if (unsubscribe != null) _unsubscribeActions.Add(unsubscribe);
        }

        ShopManager.Instance.InventoryUpdated += OnShopInventoryUpdated;
    }

    private void OnShopInventoryUpdated(ulong netId)
    {
        if (_player != null && netId == _player.NetId)
            _needsRefresh = true;
    }

    private void ClearProviderContents()
    {
        foreach (var unsubscribe in _unsubscribeActions) unsubscribe();
        _unsubscribeActions.Clear();

        ShopManager.Instance.InventoryUpdated -= OnShopInventoryUpdated;

        foreach (var content in _providerContents.Values) content.QueueFree();
        _providerContents.Clear();

        if (_contentContainer == null) return;
        foreach (var child in _contentContainer.GetChildren())
            child.QueueFree();
    }

    private void OnCloseButtonPressed()
    {
        Hide();
        QueueFree();
    }

    public override void _ExitTree()
    {
        ViewportResizeNotifier.Instance.OnViewportResized -= _ => PanelPositionHelper.ClampToViewport(_panel);
        CombatManager.Instance.CombatSetUp -= OnCombatSetUp;
        ClearProviderContents();
    }

    private void OnCombatSetUp(CombatState _)
    {
        Log.Debug("CombatSetUp, refreshing");
        _needsRefresh = true;
    }

    public void Refresh()
    {
        _needsRefresh = true;
    }

    public static PlayerOverlayPanel Show(Player player, Vector2? position = null)
    {
        ArgumentNullException.ThrowIfNull(player);
        var panel = new PlayerOverlayPanel
        {
            Name = $"PlayerOverlayPanel_{player.NetId}"
        };

        NRun.Instance?.GlobalUi.AddChild(panel);
        panel.Initialize(player);

        if (position.HasValue) panel.Position = position.Value;

        return panel;
    }
}
