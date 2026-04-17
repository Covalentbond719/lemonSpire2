using lemonSpire2.Chat.Input.Model;

namespace lemonSpire2.Chat.Input.Arguments;

public sealed class GreedyStringChatArgumentType : IChatArgumentType
{
    public string DisplayName => "text";

    public bool TryParse(string token, out object? value, out string? error)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            value = null;
            error = "Text cannot be empty.";
            return false;
        }

        value = token;
        error = null;
        return true;
    }

    public IReadOnlyList<ChatCompletionItem> GetCompletions(string partialToken)
    {
        return [];
    }
}
