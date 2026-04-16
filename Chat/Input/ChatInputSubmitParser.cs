using System.Text;
using lemonSpire2.Chat.Input.Registry;
using lemonSpire2.Chat.Message;

namespace lemonSpire2.Chat.Input.Parsing;

public sealed class ChatInputSubmitParser(ChatSubmitTokenHandlerRegistry tokenHandlers)
{
    public List<IMsgSegment> Parse(string text)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        return ParseSegments(text);
    }

    private List<IMsgSegment> ParseSegments(string text)
    {
        var segments = new List<IMsgSegment>();
        var plainText = new StringBuilder();
        if (text.Length == 0)
            return segments;

        if (tokenHandlers.Count == 0)
        {
            segments.Add(new RichTextSegment { Text = text });
            return segments;
        }

        var searchValues = tokenHandlers.CreateSearchValues();
        var cursor = 0;
        while (cursor < text.Length)
        {
            // 先跳到下一个可能触发特殊语义的位置，普通文本统一累积到 plainText。
            var relativeIndex = text.AsSpan(cursor).IndexOfAny(searchValues);
            if (relativeIndex < 0)
            {
                plainText.Append(text, cursor, text.Length - cursor);
                break;
            }

            var tokenIndex = cursor + relativeIndex;
            plainText.Append(text, cursor, tokenIndex - cursor);

            if (tokenHandlers.TryParse(text, tokenIndex, out var segment, out var consumedLength))
            {
                // 只有 handler 明确吃掉整段语法时，前面的 plainText 才会被冲刷成独立 segment。
                FlushPlainText(segments, plainText);
                segments.Add(segment);
                cursor = tokenIndex + consumedLength;
                continue;
            }

            plainText.Append(text[tokenIndex]);
            cursor = tokenIndex + 1;
        }

        FlushPlainText(segments, plainText);
        return segments;
    }

    private static void FlushPlainText(List<IMsgSegment> segments, StringBuilder plainText)
    {
        if (plainText.Length == 0)
            return;

        segments.Add(new RichTextSegment { Text = plainText.ToString() });
        plainText.Clear();
    }
}
