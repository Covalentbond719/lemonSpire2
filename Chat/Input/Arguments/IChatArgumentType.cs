using lemonSpire2.Chat.Input.Model;

namespace lemonSpire2.Chat.Input.Arguments;

public interface IChatArgumentType
{
    string DisplayName { get; }
    bool TryParse(string token, out object? value, out string? error);
    IReadOnlyList<ChatCompletionItem> GetCompletions(string partialToken);
}
