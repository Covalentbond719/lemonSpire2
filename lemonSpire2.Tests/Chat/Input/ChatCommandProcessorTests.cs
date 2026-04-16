using lemonSpire2.Chat.Input.Command;
using lemonSpire2.Chat.Input.Model;
using lemonSpire2.Chat.Input.Service.Command;
using lemonSpire2.Chat.Message;
using Xunit;

namespace lemonSpire2.Tests.Chat.Input;

public sealed class ChatCmdProcessorTests
{
    [Fact]
    public void Process_ShouldReturnHelpListing_ForHelpCommandWithoutArguments()
    {
        var processor = CreateProcessor();

        var result = processor.Process("/help");

        var display = Assert.IsType<LocalDisplayChatCmdResult>(result);
        Assert.False(string.IsNullOrWhiteSpace(display.Text));
        Assert.Equal(ChatCmdText.SystemHeader(), display.HeaderText);
    }

    [Fact]
    public void Process_ShouldReturnSendSegments_ForWhisperCommand()
    {
        var processor = CreateProcessor(
        [
            new MentionTarget("Alice", 42, () => new RichTextSegment { Text = "<alice>" })
            {
                MentionText = "Alice_1"
            }
        ]);

        var result = processor.Process("/w @Alice_1 hello there");

        var send = Assert.IsType<SendSegmentsChatCmdResult>(result);
        Assert.Collection(send.Messages,
            message =>
            {
                Assert.Equal<ulong>(42, message.ReceiverId);
                var segment = Assert.IsType<RichTextSegment>(Assert.Single(message.Segments));
                Assert.Equal("hello there", segment.Text);
            },
            message =>
            {
                Assert.Equal<ulong>(7, message.ReceiverId);
                var segment = Assert.IsType<RichTextSegment>(Assert.Single(message.Segments));
                Assert.Equal("hello there", segment.Text);
            });
    }

    [Fact]
    public void Process_ShouldNotDuplicateWhisper_WhenTargetIsLocalPlayer()
    {
        var processor = CreateProcessor(
        [
            new MentionTarget("Alice", 7, () => new RichTextSegment { Text = "<alice>" })
            {
                MentionText = "Alice_1"
            }
        ]);

        var result = processor.Process("/w @Alice_1 hello there");

        var send = Assert.IsType<SendSegmentsChatCmdResult>(result);
        var message = Assert.Single(send.Messages);
        Assert.Equal<ulong>(7, message.ReceiverId);
        var segment = Assert.IsType<RichTextSegment>(Assert.Single(message.Segments));
        Assert.Equal("hello there", segment.Text);
    }

    [Fact]
    public void Process_ShouldReturnError_ForUnknownCommand()
    {
        var processor = CreateProcessor();

        var result = processor.Process("/foobar hi");

        var error = Assert.IsType<ErrorChatCmdResult>(result);
        Assert.False(string.IsNullOrWhiteSpace(error.Message));
        Assert.Equal(ChatCmdText.SystemHeader(), error.HeaderText);
    }

    [Fact]
    public void Completion_ShouldSuggestCommandNames_AfterSlash()
    {
        var analyzer = CreateSlashAnalyzer();

        Assert.True(analyzer.TryAnalyze("/", 1, out var session));

        var items = session.Provider.GetItems(session.Query);
        Assert.Contains(items, item => item.InsertText == "/help ");
        Assert.Contains(items, item => item.InsertText == "/w ");
    }

    [Fact]
    public void Completion_ShouldSuggestPlayers_ForWhisperTarget()
    {
        var analyzer = CreateSlashAnalyzer(
        [
            new MentionTarget("Alice", 42, () => new RichTextSegment { Text = "<alice>" })
            {
                MentionText = "Alice_1"
            },
            new MentionTarget("Bob", 7, () => new RichTextSegment { Text = "<bob>" })
        ]);

        Assert.True(analyzer.TryAnalyze("/w @A", "/w @A".Length, out var session));

        var items = session.Provider.GetItems(session.Query);
        Assert.Contains(items, item => item.InsertText == "@Alice_1 ");
    }

    private static ChatCmdProcessor CreateProcessor(IReadOnlyList<MentionTarget>? mentionTargets = null)
    {
        var registry = CreateRegistry(mentionTargets);
        return new ChatCmdProcessor(registry);
    }

    private static SlashCommandCompletionAnalyzer CreateSlashAnalyzer(
        IReadOnlyList<MentionTarget>? mentionTargets = null)
    {
        return new SlashCommandCompletionAnalyzer(CreateRegistry(mentionTargets));
    }

    private static ChatCmdRegistry CreateRegistry(IReadOnlyList<MentionTarget>? mentionTargets = null)
    {
        var registry = new ChatCmdRegistry();
        var targets = mentionTargets ?? [];
        DefaultChatCmds.RegisterDefaults(registry, () => targets, () => 7,
            text => [new RichTextSegment { Text = text }]);
        return registry;
    }
}
