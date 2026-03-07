using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Localization.Fonts;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.addons.mega_text;

namespace lemonSpire2.Chat;

/// <summary>
/// 聊天UI组件 - 显示在屏幕左下角
/// </summary>
public partial class ChatUI : Control
{
    private const int MaxVisibleMessages = 8;
    private const float MessageFadeTime = 10f;
    private const float ChatWidth = 300f;
    private const float InputHeight = 30f;
    private const float MessageHeight = 22f;

    private VBoxContainer _messageContainer = null!;
    private ScrollContainer _scrollContainer = null!;
    private LineEdit _inputField = null!;
    private Control _inputContainer = null!;
    private Button _sendButton = null!;
    private Button _toggleButton = null!;

    private readonly List<ChatMessageEntry> _messages = new();
    private bool _isExpanded = true;
    private bool _isInputFocused = false;
    private double _lastMessageTime = 0;

    public event System.Action<string>? OnMessageSent;

    public override void _Ready()
    {
        AnchorLeft = 0;
        AnchorTop = 1;
        AnchorRight = 0;
        AnchorBottom = 1;

        OffsetLeft = 10;
        OffsetTop = -300;
        OffsetRight = 10 + ChatWidth;
        OffsetBottom = -10;

        MouseFilter = MouseFilterEnum.Ignore;

        CreateUI();
    }

    private void CreateUI()
    {
        // 主容器
        var mainContainer = new VBoxContainer
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill,
            MouseFilter = MouseFilterEnum.Ignore
        };
        AddChild(mainContainer);

        // 切换按钮
        _toggleButton = new Button
        {
            Text = "💬",
            CustomMinimumSize = new Vector2(40, 30),
            ToggleMode = true,
            ButtonPressed = true,
            MouseFilter = MouseFilterEnum.Stop
        };
        _toggleButton.Connect(Button.SignalName.Toggled, Callable.From<bool>(OnTogglePressed));
        mainContainer.AddChild(_toggleButton);

        // 可折叠内容区域
        _scrollContainer = new ScrollContainer
        {
            SizeFlagsVertical = SizeFlags.ExpandFill,
            CustomMinimumSize = new Vector2(ChatWidth, 200),
            MouseFilter = MouseFilterEnum.Stop
        };
        _scrollContainer.AddThemeStyleboxOverride("panel", CreatePanelStyle(new Color(0, 0, 0, 0.7f)));
        mainContainer.AddChild(_scrollContainer);

        _messageContainer = new VBoxContainer
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        _scrollContainer.AddChild(_messageContainer);

        // 输入区域容器
        _inputContainer = new HBoxContainer
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            CustomMinimumSize = new Vector2(ChatWidth, InputHeight),
            MouseFilter = MouseFilterEnum.Stop
        };
        _inputContainer.AddThemeStyleboxOverride("panel", CreatePanelStyle(new Color(0, 0, 0, 0.8f)));
        mainContainer.AddChild(_inputContainer);

        // 输入框
        _inputField = new LineEdit
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            PlaceholderText = ModLocalization.Get("chat.placeholder", "Type a message..."),
            MouseFilter = MouseFilterEnum.Stop
        };
        _inputField.AddThemeColorOverride("font_color", Colors.White);
        _inputField.AddThemeColorOverride("font_placeholder_color", new Color(0.7f, 0.7f, 0.7f));
        _inputField.AddThemeColorOverride("background_color", new Color(0.1f, 0.1f, 0.1f, 0.9f));
        _inputField.Connect(LineEdit.SignalName.TextSubmitted, Callable.From<string>(OnTextSubmitted));
        _inputField.Connect(LineEdit.SignalName.FocusEntered, Callable.From(() => OnInputFocusEntered()));
        _inputField.Connect(LineEdit.SignalName.FocusExited, Callable.From(() => OnInputFocusExited()));
        _inputField.ApplyLocaleFontSubstitution(FontType.Regular, "font");
        _inputContainer.AddChild(_inputField);

        // 发送按钮
        _sendButton = new Button
        {
            Text = ModLocalization.Get("chat.send", "Send"),
            CustomMinimumSize = new Vector2(60, 0),
            MouseFilter = MouseFilterEnum.Stop
        };
        _sendButton.Connect(Button.SignalName.Pressed, Callable.From(OnSendPressed));
        _inputContainer.AddChild(_sendButton);
    }

    private StyleBoxFlat CreatePanelStyle(Color bgColor)
    {
        return new StyleBoxFlat
        {
            BgColor = bgColor,
            BorderColor = new Color(0.3f, 0.3f, 0.3f),
            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,
            CornerRadiusBottomLeft = 4,
            CornerRadiusBottomRight = 4,
            CornerRadiusTopLeft = 4,
            CornerRadiusTopRight = 4,
            ContentMarginLeft = 4,
            ContentMarginRight = 4,
            ContentMarginTop = 2,
            ContentMarginBottom = 2
        };
    }

    public void AddMessage(ulong senderId, string senderName, string content, long timestamp)
    {
        var entry = new ChatMessageEntry(senderId, senderName, content, timestamp);
        _messages.Add(entry);

        var label = CreateMessageLabel(entry);
        _messageContainer.AddChild(label);

        // 限制消息数量
        while (_messages.Count > 50)
        {
            _messages.RemoveAt(0);
            var firstChild = _messageContainer.GetChild(0);
            firstChild?.QueueFree();
        }

        // 滚动到底部
        CallDeferred(nameof(ScrollToBottom));
        _lastMessageTime = Time.GetTicksMsec() / 1000.0;

        // 如果折叠状态，自动展开
        if (!_isExpanded)
        {
            SetExpanded(true);
        }
    }

    private Label CreateMessageLabel(ChatMessageEntry entry)
    {
        var label = new Label
        {
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            MouseFilter = MouseFilterEnum.Ignore
        };
        label.AddThemeColorOverride("font_color", Colors.White);
        label.AddThemeFontSizeOverride("font_size", 14);

        // 区分自己和他人消息的颜色
        var netService = RunManager.Instance?.NetService;
        if (netService != null && entry.SenderId == netService.NetId)
        {
            label.AddThemeColorOverride("font_color", new Color(0.6f, 1f, 0.6f)); // 自己的消息 - 浅绿色
        }
        else
        {
            label.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 1f)); // 他人消息 - 浅蓝色
        }

        label.Text = $"[{entry.SenderName}]: {entry.Content}";
        return label;
    }

    private void ScrollToBottom()
    {
        if (_scrollContainer != null)
        {
            _scrollContainer.ScrollVertical = (int)_scrollContainer.GetVScrollBar().MaxValue;
        }
    }

    private void OnTogglePressed(bool pressed)
    {
        SetExpanded(pressed);
    }

    private void SetExpanded(bool expanded)
    {
        _isExpanded = expanded;
        _scrollContainer.Visible = expanded;
        _inputContainer.Visible = expanded;
        _toggleButton.ButtonPressed = expanded;
    }

    private void OnTextSubmitted(string text)
    {
        SendMessage(text);
    }

    private void OnSendPressed()
    {
        SendMessage(_inputField.Text);
    }

    private void SendMessage(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        OnMessageSent?.Invoke(text.Trim());
        _inputField.Text = "";
        _inputField.ReleaseFocus();
    }

    private void OnInputFocusEntered()
    {
        _isInputFocused = true;
    }

    private void OnInputFocusExited()
    {
        _isInputFocused = false;
    }

    public override void _Input(InputEvent @event)
    {
        // 按 Enter 聚焦输入框（如果未聚焦）
        if (@event.IsActionPressed(MegaInput.select) && !_isInputFocused && _isExpanded)
        {
            var isChatVisible = Visible && GetViewport()?.GuiGetFocusOwner() == null;
            if (isChatVisible && IsInstanceValid(_inputField))
            {
                _inputField.GrabFocus();
                GetViewport()?.SetInputAsHandled();
            }
        }

        // 按 Escape 取消聚焦
        if (@event.IsActionPressed(MegaInput.cancel) && _isInputFocused)
        {
            _inputField.ReleaseFocus();
            GetViewport()?.SetInputAsHandled();
        }
    }

    public override void _Process(double delta)
    {
        // 自动折叠逻辑：如果消息区域空闲超过一定时间，自动折叠
        // 这里暂不实现，保持展开状态便于查看
    }

    public void SetVisibleForMultiplayer(bool isMultiplayer)
    {
        Visible = isMultiplayer;
    }
}

/// <summary>
/// 聊天消息条目
/// </summary>
public record ChatMessageEntry(ulong SenderId, string SenderName, string Content, long Timestamp);
