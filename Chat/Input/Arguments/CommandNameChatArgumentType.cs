using lemonSpire2.Chat.Input.Model;
using lemonSpire2.Chat.Input.Command;

namespace lemonSpire2.Chat.Input.Arguments;

public sealed class CommandNameChatArgumentType(ChatCmdRegistry registry) : IChatArgumentType
{
    public string DisplayName => "command";

    public bool TryParse(string token, out object? value, out string? error)
    {
        if (registry.TryGet(token, out var command) && command is not null)
        {
            value = command.Name;
            error = null;
            return true;
        }

        value = null;
        error = $"Unknown command '{token}'.";
        return false;
    }

    public IReadOnlyList<ChatCompletionItem> GetCompletions(string partialToken)
    {
        return
        [
            .. registry.All
                .Where(command => command.Name.Contains(partialToken, StringComparison.OrdinalIgnoreCase))
                .Select(command => new ChatCompletionItem(command.Name, $"{command.Name} "))
        ];
    }
}
