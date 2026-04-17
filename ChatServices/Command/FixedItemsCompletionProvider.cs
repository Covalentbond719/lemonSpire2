using lemonSpire2.Chat.Input.Abstractions;
using lemonSpire2.Chat.Input.Model;

namespace lemonSpire2.Chat.Input.Service.Command;

internal sealed class FixedItemsCompletionProvider(IReadOnlyList<ChatCompletionItem> items) : IChatCompletionProvider
{
    public IReadOnlyList<ChatCompletionItem> GetItems(string query)
    {
        return items;
    }
}
