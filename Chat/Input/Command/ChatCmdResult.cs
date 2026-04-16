using lemonSpire2.Chat.Message;

namespace lemonSpire2.Chat.Input.Command;

public abstract record ChatCmdResult;

public sealed record ErrorChatCmdResult(string Message, string HeaderText = "System") : ChatCmdResult;

public sealed record LocalDisplayChatCmdResult(string Text, string HeaderText = "System") : ChatCmdResult;

public sealed record NotAChatCmdResult : ChatCmdResult;

public sealed record ChatCmdOutgoingMessage(ulong ReceiverId, IReadOnlyCollection<IMsgSegment> Segments);

public sealed record SendSegmentsChatCmdResult(IReadOnlyCollection<ChatCmdOutgoingMessage> Messages) : ChatCmdResult;
