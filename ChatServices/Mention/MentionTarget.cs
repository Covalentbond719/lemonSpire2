using lemonSpire2.Chat.Input.Service.Mention;
using lemonSpire2.Chat.Message;

namespace lemonSpire2.Chat.Input.Model;

public sealed record MentionTarget(string DisplayName, ulong PlayerNetId, Func<IMsgSegment> CreateSegment)
{
    public string MentionText { get; init; } = MentionTextCodec.Encode(DisplayName);

    public string GetDisplayText()
    {
        var defaultMention = MentionTextCodec.Encode(DisplayName);
        return string.Equals(defaultMention, MentionText, StringComparison.Ordinal)
            ? DisplayName
            : $"{DisplayName} (@{MentionText})";
    }
}
