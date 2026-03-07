using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Runs;

namespace lemonSpire2.Chat;

/// <summary>
/// 聊天管理器 - 负责消息收发和网络同步
/// </summary>
public class ChatManager : IDisposable
{
    private static ChatManager? _instance;
    public static ChatManager Instance => _instance ??= new ChatManager();

    private INetGameService? _netService;
    private ChatUI? _chatUI;
    private bool _isInitialized = false;

    private readonly List<ChatMessageEntry> _messageHistory = new();
    private const int MaxHistorySize = 100;

    public event Action<ChatMessageEntry>? OnMessageReceived;

    private ChatManager() { }

    public void Initialize(INetGameService netService)
    {
        if (_isInitialized)
        {
            Cleanup();
        }

        _netService = netService;
        _netService.RegisterMessageHandler<ChatMessage>(OnChatMessageReceived);
        _isInitialized = true;

        MainFile.Logger.Info("ChatManager initialized");
    }

    public void SetChatUI(ChatUI chatUI)
    {
        _chatUI = chatUI;
        _chatUI.OnMessageSent += OnLocalMessageSent;

        // 更新可见性
        UpdateVisibility();
    }

    private void OnChatMessageReceived(ChatMessage message, ulong senderId)
    {
        var entry = new ChatMessageEntry(
            message.senderId,
            message.senderName,
            message.content,
            message.timestamp
        );

        AddToHistory(entry);
        OnMessageReceived?.Invoke(entry);

        // 在主线程更新UI
        if (_chatUI != null && Godot.GodotObject.IsInstanceValid(_chatUI))
        {
            _chatUI.CallDeferred(nameof(ChatUI.AddMessage),
                message.senderId, message.senderName, message.content, message.timestamp);
        }

        MainFile.Logger.Debug($"Chat message received from {message.senderName}: {message.content}");
    }

    private void OnLocalMessageSent(string content)
    {
        if (_netService == null || !_netService.IsConnected)
        {
            MainFile.Logger.Warn("Cannot send chat message: not connected");
            return;
        }

        // 获取当前玩家名称
        string playerName = GetLocalPlayerName();

        var message = new ChatMessage
        {
            senderId = _netService.NetId,
            senderName = playerName,
            content = content,
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        // 发送到网络
        _netService.SendMessage(message);

        // 本地也显示（因为消息会广播回来，但自己发的会立即显示）
        // 注意：根据 ShouldBroadcast = true，消息会广播给所有人包括自己
        // 所以这里不需要手动添加，等待广播回调即可
        // 但为了更好的用户体验，我们立即显示自己发的消息
        var entry = new ChatMessageEntry(message.senderId, message.senderName, message.content, message.timestamp);
        AddToHistory(entry);

        MainFile.Logger.Debug($"Chat message sent: {content}");
    }

    private string GetLocalPlayerName()
    {
        if (_netService == null)
            return "Player";

        return PlatformUtil.GetPlayerName(_netService.Platform, _netService.NetId) ?? "Player";
    }

    private void AddToHistory(ChatMessageEntry entry)
    {
        _messageHistory.Add(entry);

        while (_messageHistory.Count > MaxHistorySize)
        {
            _messageHistory.RemoveAt(0);
        }
    }

    public IReadOnlyList<ChatMessageEntry> GetMessageHistory() => _messageHistory;

    public void UpdateVisibility()
    {
        if (_chatUI != null && Godot.GodotObject.IsInstanceValid(_chatUI))
        {
            bool isMultiplayer = _netService != null && _netService.Type.IsMultiplayer();
            _chatUI.CallDeferred(nameof(ChatUI.SetVisibleForMultiplayer), isMultiplayer);
        }
    }

    public void Cleanup()
    {
        if (_netService != null)
        {
            _netService.UnregisterMessageHandler<ChatMessage>(OnChatMessageReceived);
        }

        if (_chatUI != null)
        {
            _chatUI.OnMessageSent -= OnLocalMessageSent;
        }

        _messageHistory.Clear();
        _isInitialized = false;

        MainFile.Logger.Info("ChatManager cleaned up");
    }

    public void Dispose()
    {
        Cleanup();
        _instance = null;
    }
}
