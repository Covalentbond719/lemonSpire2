using lemonSpire2.Chat.Input.Abstractions;
using lemonSpire2.Chat.Input.Command;
using lemonSpire2.Chat.Input.Model;
using lemonSpire2.Chat.Input.Parsing;

namespace lemonSpire2.Chat.Input.Service.Command;

public sealed class SlashCommandCompletionAnalyzer(ChatCmdRegistry registry) : IChatCompletionAnalyzer
{
    public bool TryAnalyze(string text, int caretColumn, out ChatCompletionSession session)
    {
        ArgumentNullException.ThrowIfNull(text);
        session = null!;
        if (!text.StartsWith('/') || caretColumn <= 0 || caretColumn > text.Length)
            return false;

        var prefix = text[..caretColumn];
        // Slash completion 完全基于“光标左边已输入的 token”决定当前是在补命令名还是补第 N 个参数。
        var tokenizeResult = ChatCmdTokenizer.Tokenize(prefix);
        if (tokenizeResult.Tokens.Count == 0)
        {
            session = new ChatCompletionSession(0, caretColumn, string.Empty,
                new SlashCommandCompletionProvider(registry));
            return true;
        }

        var commandToken = tokenizeResult.Tokens[0];
        if (tokenizeResult.Tokens.Count == 1 && !tokenizeResult.HasTrailingWhitespace)
        {
            session = new ChatCompletionSession(0, caretColumn,
                commandToken.Value, new SlashCommandCompletionProvider(registry));
            return true;
        }

        if (!registry.TryGet(commandToken.Value, out var command) || command is null)
            return false;

        var argTokens = tokenizeResult.Tokens.Skip(1).ToArray();
        var argIndex = tokenizeResult.HasTrailingWhitespace ? argTokens.Length : argTokens.Length - 1;
        if (argIndex < 0 || argIndex >= command.Args.Count)
            return false;

        // Replace 区间只覆盖“当前正在编辑的那个 token”，这样 popup 确认时不会重写前面的参数。
        var spec = command.Args[argIndex];
        var currentToken = tokenizeResult.HasTrailingWhitespace
            ? string.Empty
            : argTokens[argIndex].Value;
        var replaceStart = tokenizeResult.HasTrailingWhitespace
            ? caretColumn
            : argTokens[argIndex].Start;
        var replaceLength = tokenizeResult.HasTrailingWhitespace
            ? 0
            : argTokens[argIndex].Length;

        session = new ChatCompletionSession(
            replaceStart,
            replaceLength,
            currentToken,
            new FixedItemsCompletionProvider(spec.ArgType.GetCompletions(currentToken)));
        return true;
    }
}
