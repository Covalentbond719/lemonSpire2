using Godot;
using lemonSpire2.Chat.Intent;
using lemonSpire2.Tooltips;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Nodes.HoverTips;

namespace lemonSpire2.Chat.Ui;

/// <summary>
///     Manages tooltip preview display using NHoverTipSet.
///     Positions tooltip with left-center alignment relative to mouse cursor.
/// </summary>
public sealed class TooltipManager : IDisposable
{
    private Control? _currentOwner;
    private NHoverTipSet? _currentTipSet;
    private bool _disposed;
    private Control? _parent;

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        ClearPreview();
        _currentOwner = null;
        _parent = null;
    }

    public void RegisterHandlers(IntentHandlerRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);
        registry.Register<IntentMetaHoverStart>(OnHoverStart);
        registry.Register<IntentMetaHoverEnd>(OnHoverEnd);
        registry.Register<IntentMetaClick>(OnClick);
    }

    public void Initialize(Control parent)
    {
        _parent = parent;
    }

    public void UpdatePreviewPosition(Vector2 globalMousePosition)
    {
        if (_currentTipSet is null) return;

        var viewport = _currentTipSet.GetViewportRect().Size;

        // Get internal containers
        var textContainer = _currentTipSet.GetNode<VFlowContainer>("textHoverTipContainer");
        var cardContainer = _currentTipSet.GetNode<NHoverTipCardContainer>("cardHoverTipContainer");

        // Card container needs LayoutResizeAndReposition to calculate its size
        // Call it with a provisional position to trigger size calculation
        if (cardContainer is not null && cardContainer.GetChildCount() > 0)
        {
            cardContainer.LayoutResizeAndReposition(globalMousePosition, HoverTipAlignment.Right);
        }

        _currentTipSet.ResetSize();

        // Get actual sizes
        var textHeight = textContainer?.Size.Y ?? 0;
        var cardHeight = cardContainer?.Size.Y ?? 0;
        var textWidth = textContainer?.Size.X ?? 0;
        var cardWidth = cardContainer?.Size.X ?? 0;

        // Total dimensions - containers are positioned side by side or stacked
        var totalHeight = Math.Max(textHeight, cardHeight);
        var totalWidth = Math.Max(textWidth, cardWidth);

        // Fallback to outer control size
        if (totalHeight <= 0 || totalWidth <= 0)
        {
            totalWidth = _currentTipSet.Size.X;
            totalHeight = _currentTipSet.Size.Y;
        }

        // Left-center alignment
        var tipX = globalMousePosition.X + 16;
        var tipY = globalMousePosition.Y - totalHeight / 2;

        // Clamp Y
        if (tipY < 0) tipY = 0;
        else if (tipY + totalHeight > viewport.Y)
            tipY = viewport.Y - totalHeight;

        // Move to left of cursor if overflowing right edge
        if (tipX + totalWidth > viewport.X) tipX = globalMousePosition.X - totalWidth - 8;

        _currentTipSet.GlobalPosition = new Vector2(tipX, tipY);

        // Reposition card container relative to the new tip position
        if (cardContainer is not null && cardContainer.GetChildCount() > 0)
        {
            var alignment = tipX > globalMousePosition.X ? HoverTipAlignment.Right : HoverTipAlignment.Left;
            cardContainer.LayoutResizeAndReposition(
                _currentTipSet.GlobalPosition + new Vector2(0, textHeight),
                alignment);
        }
    }

    private void OnHoverStart(IntentMetaHoverStart intent)
    {
        if (_disposed || _parent is null) return;

        ClearPreview();

        var tooltip = Tooltip.FromMetaString(intent.Meta);
        if (tooltip is null)
        {
            MainFile.Logger.Warn($"Failed to resolve tooltip from meta: {intent.Meta}");
            return;
        }

        try
        {
            var hoverTip = tooltip.ToHoverTip();

            // Create a dummy owner for NHoverTipSet registry
            _currentOwner = new Control { Name = "TooltipOwner" };
            _parent.AddChild(_currentOwner);

            _currentTipSet = NHoverTipSet.CreateAndShow(_currentOwner, hoverTip);

            // Position after layout completes (deferred to next frame)
            var pos = intent.GlobalPosition;
            Callable.From(() =>
            {
                _currentTipSet!.GlobalPosition = pos + new Vector2(16, 0);
                UpdatePreviewPosition(pos);
            }).CallDeferred();
        }
        catch (Exception ex)
        {
            MainFile.Logger.Error($"Failed to create tooltip preview: {ex.Message}");
        }
    }

    private void OnHoverEnd(IntentMetaHoverEnd intent)
    {
        if (_disposed) return;
        ClearPreview();
    }

    private void OnClick(IntentMetaClick intent)
    {
        if (_disposed) return;
        ClearPreview();
    }

    private void ClearPreview()
    {
        if (_currentOwner is not null)
        {
            NHoverTipSet.Remove(_currentOwner);
            _currentOwner.QueueFree();
            _currentOwner = null;
        }
        _currentTipSet = null;
    }
}