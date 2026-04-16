using lemonSpire2.Chat.Input.Model;
using lemonSpire2.Chat.Input.Abstractions;

namespace lemonSpire2.Chat.Input.Service.Bracket;

public sealed class InlineReferenceCompletionAnalyzer(IChatCompletionProvider provider) : IChatCompletionAnalyzer
{
    public bool TryAnalyze(string text, int caretColumn, out ChatCompletionSession session)
    {
        ArgumentNullException.ThrowIfNull(text);
        session = null!;
        var leaderIndex = FindLeaderIndex(text, caretColumn);
        if (leaderIndex < 0)
            return false;

        // 光标右边如果已经存在一个闭合的 `>`，说明这段 `<...>` 已经成型，不再弹补全。
        if (HasClosedReferenceAhead(text, leaderIndex))
            return false;

        session = new ChatCompletionSession(leaderIndex, caretColumn - leaderIndex,
            text[(leaderIndex + 1)..caretColumn], provider);
        return true;
    }

    private static int FindLeaderIndex(string text, int caretColumn)
    {
        for (var index = caretColumn - 1; index >= 0; index--)
        {
            var c = text[index];
            if (c == '<')
                return index;

            // 一旦跨过空白或另一个 `>`，就说明当前光标不在同一个 inline-ref 词法片段里。
            if (c == '>' || char.IsWhiteSpace(c))
                break;
        }

        return -1;
    }

    private static bool HasClosedReferenceAhead(string text, int leaderIndex)
    {
        const char closingChar = '>';
        var nextLeft = text.IndexOf('<', leaderIndex + 1);
        var nextRight = text.IndexOf(closingChar, leaderIndex + 1);
        return (nextLeft == -1 && nextRight != -1) || (nextLeft >= 0 && nextLeft > nextRight && nextRight >= 0);
    }
}
