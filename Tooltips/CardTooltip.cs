using Godot;
using lemonSpire2.util;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace lemonSpire2.Tooltips;

public sealed class CardTooltip : Tooltip
{
    protected override string TypeTag => "card";

    public required string ModelIdStr { get; set; }
    public int UpgradeLevel { get; set; }

    public static CardTooltip FromModel(CardModel card)
    {
        ArgumentNullException.ThrowIfNull(card);
        return new CardTooltip
        {
            ModelIdStr = card.Id.Entry,
            UpgradeLevel = card.CurrentUpgradeLevel
        };
    }

    public static Color GetCardRarityColor(CardRarity rarity)
    {
        return rarity switch
        {   
            CardRarity.Basic => StsColors.cardTitleOutlineCommon, // Basic cards use the same color as Common
            CardRarity.Common => StsColors.cardTitleOutlineCommon,
            CardRarity.Uncommon => StsColors.cardTitleOutlineUncommon,
            CardRarity.Rare => StsColors.cardTitleOutlineRare,
            CardRarity.Curse => StsColors.cardTitleOutlineCurse,
            CardRarity.Quest => StsColors.cardTitleOutlineQuest,
            CardRarity.Status => StsColors.cardTitleOutlineStatus,
            CardRarity.Ancient => StsColors.cardTitleOutlineSpecial, // if  
            CardRarity.Event => StsColors.cardTitleOutlineSpecial, // 
            CardRarity.Token => StsColors.cardTitleOutlineSpecial,
            CardRarity.None => StsColors.cream,
            _ => throw new ArgumentOutOfRangeException(nameof(rarity), rarity, null)
        };
    }
    
    public static Color GetCardPoolColor(CardModel card)
    {
        return card.VisualCardPool.DeckEntryCardColor;
    }
    
    public override string Render()
    {
        var card = ResolveModel();
        if (card is null) return "Broken Card";

        var rarityColor = GetCardRarityColor(card.Rarity);
        var poolColor = GetCardPoolColor(card);

        return $"[color={poolColor.ToHtml()}]■[/color] [color={rarityColor.ToHtml()}]{card.Title}[/color]";
    }

    public override void Serialize(PacketWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteString(ModelIdStr);
        writer.WriteInt(UpgradeLevel);
    }

    public override void Deserialize(PacketReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ModelIdStr = reader.ReadString();
        UpgradeLevel = reader.ReadInt();
    }

    public override IHoverTip ToHoverTip()
    {
        var model = ResolveModel();
        if (model is null)
            throw new InvalidOperationException($"Cannot resolve card model: {ModelIdStr}");

        // Ensure we have a mutable copy for CardHoverTip
        var mutableCard = model.IsMutable ? model : model.ToMutable();
        return new CardHoverTip(mutableCard);
    }

    private CardModel? ResolveModel()
    {
        return Util.ResolveModel<CardModel>(ModelIdStr);
    }
}
