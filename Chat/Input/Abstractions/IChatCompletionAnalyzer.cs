using lemonSpire2.Chat.Input.Model;

namespace lemonSpire2.Chat.Input.Abstractions;

public interface IChatCompletionAnalyzer
{
    bool TryAnalyze(string text, int caretColumn, out ChatCompletionSession session);
}
