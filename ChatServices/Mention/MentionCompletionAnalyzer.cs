using lemonSpire2.Chat.Input.Model;
using lemonSpire2.Chat.Input.Abstractions;

namespace lemonSpire2.Chat.Input.Service.Mention;

public sealed class MentionCompletionAnalyzer(IChatCompletionProvider provider) : IChatCompletionAnalyzer
{
    public bool TryAnalyze(string text, int caretColumn, out ChatCompletionSession session)
    {
        ArgumentNullException.ThrowIfNull(text);
        session = null!;
        var leaderIndex = text.LastIndexOf('@', caretColumn - 1, caretColumn);
        switch (leaderIndex)
        {
            // `foo@bar` 这种中间没有空白的 @ 不视为 mention 起点，避免把普通文本误判成提及。
            case < 0:
            case > 0 when !char.IsWhiteSpace(text[leaderIndex - 1]):
                return false;
        }

        var query = text[(leaderIndex + 1)..caretColumn];
        if (query.Contains(' ', StringComparison.Ordinal))
            return false;

        session = new ChatCompletionSession(leaderIndex, caretColumn - leaderIndex, query, provider);
        return true;
    }
}
