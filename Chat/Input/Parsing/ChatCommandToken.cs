namespace lemonSpire2.Chat.Input.Parsing;

public sealed record ChatCmdToken(string Value, int Start, int Length);

public sealed record ChatCmdTokenizeResult(IReadOnlyList<ChatCmdToken> Tokens, bool HasTrailingWhitespace);
