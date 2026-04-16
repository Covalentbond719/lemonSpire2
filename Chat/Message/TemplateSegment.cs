using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace lemonSpire2.Chat.Message;

public sealed record TemplateSegment : IMsgSegment
{
    public LocString Template { get; set; } = new(string.Empty, string.Empty);
    public IReadOnlyList<NamedSegment> Slots { get; set; } = [];

    public void Serialize(PacketWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer.WriteString(Template.LocTable);
        writer.WriteString(Template.LocEntryKey);
        writer.WriteInt(Slots.Count);
        foreach (var slot in Slots)
            slot.Serialize(writer);
    }

    public void Deserialize(PacketReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);

        Template = new LocString(reader.ReadString(), reader.ReadString());

        var count = reader.ReadInt();
        var slots = new List<NamedSegment>(count);
        for (var i = 0; i < count; i++)
            slots.Add(NamedSegment.Deserialize(reader));

        Slots = slots;
    }

    public string Render()
    {
        var loc = new LocString(Template.LocTable, Template.LocEntryKey);
        foreach (var slot in Slots)
        {
            if (string.IsNullOrWhiteSpace(slot.Name))
                continue;

            loc.Add(slot.Name, slot.Render());
        }

        return loc.GetFormattedText();
    }
}
