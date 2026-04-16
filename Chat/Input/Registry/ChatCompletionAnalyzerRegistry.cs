using lemonSpire2.Chat.Input.Abstractions;
using lemonSpire2.Chat.Input.Model;

namespace lemonSpire2.Chat.Input.Registry;

public sealed class ChatCompletionAnalyzerRegistry
{
    private readonly List<IChatCompletionAnalyzer> _analyzers = [];

    public void Register(IChatCompletionAnalyzer analyzer)
    {
        ArgumentNullException.ThrowIfNull(analyzer);
        _analyzers.Add(analyzer);
    }

    public bool TryAnalyze(string text, int caretColumn, out ChatCompletionSession session)
    {
        session = null!;
        if (string.IsNullOrEmpty(text) || caretColumn <= 0 || caretColumn > text.Length)
            return false;

        foreach (var analyzer in _analyzers)
            if (analyzer.TryAnalyze(text, caretColumn, out session))
                return true;

        return false;
    }
}
