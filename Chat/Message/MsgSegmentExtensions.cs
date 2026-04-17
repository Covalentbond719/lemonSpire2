namespace lemonSpire2.Chat.Message;

public static class MsgSegmentExtensions
{
    public static NamedSegment ToNamedSegment(this IMsgSegment segment, string name)
    {
        ArgumentNullException.ThrowIfNull(segment);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new NamedSegment
        {
            Name = name,
            Segment = segment
        };
    }
}
