using lemonSpire2.Chat.Input.Abstractions;
using lemonSpire2.Chat.Input.Model;
using lemonSpire2.Chat.Message;

namespace lemonSpire2.Chat.Input.Service.Mention;

public sealed class MentionSubmitTokenHandler(Func<IReadOnlyList<MentionTarget>> getMentionTargets)
    : IChatSubmitTokenHandler
{
    public char TriggerChar => '@';

    public bool TryParse(string text, int startIndex, out IMsgSegment segment, out int length)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(getMentionTargets);
        segment = null!;
        length = 0;

        if (startIndex > 0 && !char.IsWhiteSpace(text[startIndex - 1]))
            return false;

        var endIndex = startIndex + 1;
        // mention 的词法边界故意只看空白和 inline-ref 起点；其余标点保持交给 alias 文本自己处理。
        while (endIndex < text.Length && !char.IsWhiteSpace(text[endIndex]) && text[endIndex] != '<')
            endIndex++;

        if (endIndex <= startIndex + 1)
            return false;

        var name = text[(startIndex + 1)..endIndex];
        var matches = getMentionTargets()
            .Where(target => string.Equals(target.MentionText, name, StringComparison.Ordinal))
            .Take(2)
            .ToList();
        // submit 阶段要求唯一命中；模糊匹配只存在于 completion，不存在于最终解析。
        if (matches.Count != 1)
            return false;

        segment = matches[0].CreateSegment();
        length = endIndex - startIndex;
        return true;
    }
}
