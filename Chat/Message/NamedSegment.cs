using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace lemonSpire2.Chat.Message;

public sealed record NamedSegment
{
    public string Name { get; set; } = string.Empty;
    public IMsgSegment Segment { get; set; } = null!;

    public void Serialize(PacketWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentException.ThrowIfNullOrWhiteSpace(Name);
        ArgumentNullException.ThrowIfNull(Segment);

        writer.WriteString(Name);
        writer.WriteInt(SegmentTypes.ToId(Segment));
        Segment.Serialize(writer);
    }

    public static NamedSegment Deserialize(PacketReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);

        var name = reader.ReadString();
        var id = reader.ReadInt();
        if (!SegmentTypes.TryGetType(id, out var type))
            throw new InvalidOperationException($"Unknown segment type id in template slot: {id}");

        var segment = (IMsgSegment)Activator.CreateInstance(type!)!;
        segment.Deserialize(reader);

        return new NamedSegment
        {
            Name = name,
            Segment = segment
        };
    }

    public string Render()
    {
        return Segment.Render();
    }
}
