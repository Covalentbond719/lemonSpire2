using lemonSpire2.Chat.Input.Abstractions;
using lemonSpire2.Chat.Input.Command;
using lemonSpire2.Chat.Input.Model;

namespace lemonSpire2.Chat.Input.Service.Command;

internal sealed class SlashCommandCompletionProvider(ChatCmdRegistry registry) : IChatCompletionProvider
{
    public IReadOnlyList<ChatCompletionItem> GetItems(string query)
    {
        return
        [
            .. registry.All
                .Where(command => command.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
                .Select(command => new ChatCompletionItem(command.Name, $"/{command.Name} "))
        ];
    }
}
