namespace lemonSpire2.Chat.Input.Parsing;

public static class ChatCmdTokenizer
{
    public static ChatCmdTokenizeResult Tokenize(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        var tokens = new List<ChatCmdToken>();
        // Slash 只是命令前缀，不属于命令名 token 本身。
        var index = text.StartsWith('/') ? 1 : 0;

        while (index < text.Length)
        {
            while (index < text.Length && char.IsWhiteSpace(text[index]))
                index++;

            if (index >= text.Length)
                break;

            var start = index;
            if (text[index] == '"')
            {
                // 简单支持带空格参数；这里不做转义语义，只负责把一对引号里的文本视为一个 token。
                index++;
                var valueStart = index;
                while (index < text.Length && text[index] != '"')
                    index++;

                var valueLength = index - valueStart;
                if (index < text.Length && text[index] == '"')
                    index++;

                tokens.Add(new ChatCmdToken(text.Substring(valueStart, valueLength), start, index - start));
                continue;
            }

            while (index < text.Length && !char.IsWhiteSpace(text[index]))
                index++;

            tokens.Add(new ChatCmdToken(text.Substring(start, index - start), start, index - start));
        }

        // 末尾空白会影响 completion：`/w ` 意味着“正在补下一个参数”，而不是还在补命令名。
        return new ChatCmdTokenizeResult(tokens, text.Length > 0 && char.IsWhiteSpace(text[^1]));
    }
}
