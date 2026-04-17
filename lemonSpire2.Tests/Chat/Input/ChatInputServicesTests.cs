using lemonSpire2.Chat.Input;
using lemonSpire2.Chat.Input.Abstractions;
using lemonSpire2.Chat.Input.Model;
using lemonSpire2.Chat.Message;
using Xunit;

namespace lemonSpire2.Tests.Chat.Input;

public sealed class ChatInputServicesTests
{
    [Fact]
    public void Constructor_ShouldRegisterProvidedInlineReferences()
    {
        var services = new ChatInputServices(
            [],
            [
                new FakeInlineReference("custom")
            ]);

        Assert.True(services.CompletionAnalyzer.TryAnalyze("<cus", "<cus".Length, out var session));

        var items = session.Provider.GetItems(session.Query);
        Assert.Contains(items, static item => item.InsertText == "<custom:");
    }

    private sealed class FakeInlineReference(string typeName) : IChatInlineReference
    {
        public string TypeName => typeName;

        public IReadOnlyList<ChatCompletionItem> GetCompletions(string query)
        {
            return
            [
                new ChatCompletionItem($"{query}-display", $"<{typeName}:{query}>")
            ];
        }

        public bool TryResolve(string payload, out IMsgSegment segment)
        {
            segment = new RichTextSegment { Text = $"<{typeName}:{payload}>" };
            return true;
        }
    }
}
