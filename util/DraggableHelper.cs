using Godot;

namespace lemonSpire2.util;

/// <summary>
///     可拖拽工具类 - 附加到 Control 上实现拖拽功能
///     使用方式：通过 AttachTo 静态方法附加到目标控件
/// </summary>
public partial class DraggableHelper : Control
{
    /// <summary>
    ///     是否正在拖拽
    /// </summary>
    public bool IsDragging { get; private set; }

    /// <summary>
    ///     拖拽偏移量
    /// </summary>
    private Vector2 _dragOffset;

    /// <summary>
    ///     父节点（被拖拽的目标）
    /// </summary>
    private Control? _target;

    /// <summary>
    ///     是否启用拖拽
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    ///     拖拽开始时的回调
    /// </summary>
    public Action? OnDragStart;

    /// <summary>
    ///     拖拽结束时的回调
    /// </summary>
    public Action? OnDragEnd;

    public override void _Ready()
    {
        // 设置为覆盖整个父节点区域
        MouseFilter = MouseFilterEnum.Pass;
        AnchorsPreset = (int)LayoutPreset.FullRect;
        GrowHorizontal = GrowDirection.Both;
        GrowVertical = GrowDirection.Both;

        _target = GetParent<Control>();
    }

    public override void _Input(InputEvent @event)
    {
        if (!Enabled || _target == null) return;

        // 鼠标左键按下 - 开始拖拽
        if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left })
        {
            var mousePos = GetGlobalMousePosition();
            if (IsMouseOverTarget(mousePos))
            {
                StartDrag(mousePos);
                GetViewport()?.SetInputAsHandled();
            }
        }

        // 鼠标左键释放 - 结束拖拽
        if (@event is InputEventMouseButton { Pressed: false, ButtonIndex: MouseButton.Left } && IsDragging)
        {
            EndDrag();
        }
    }

    public override void _Process(double delta)
    {
        if (!IsDragging || _target == null) return;

        var mousePos = GetGlobalMousePosition();
        _target.GlobalPosition = mousePos - _dragOffset;
    }

    private bool IsMouseOverTarget(Vector2 globalMousePos)
    {
        if (_target == null) return false;

        var rect = new Rect2(_target.GlobalPosition, _target.Size);
        return rect.HasPoint(globalMousePos);
    }

    private void StartDrag(Vector2 mousePos)
    {
        if (_target == null) return;

        IsDragging = true;
        _dragOffset = mousePos - _target.GlobalPosition;
        OnDragStart?.Invoke();

        // 提升到最前层
        _target.MoveToFront();
    }

    private void EndDrag()
    {
        IsDragging = false;
        OnDragEnd?.Invoke();
    }

    /// <summary>
    ///     静态工厂方法 - 创建并附加到目标
    /// </summary>
    public static DraggableHelper AttachTo(Control target, Action? onDragStart = null, Action? onDragEnd = null)
    {
        var helper = new DraggableHelper
        {
            Name = "DraggableHelper",
            OnDragStart = onDragStart,
            OnDragEnd = onDragEnd
        };

        target.AddChild(helper);
        return helper;
    }
}