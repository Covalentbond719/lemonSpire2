using lemonSpire2.PlayerStateEx.RemoteFlash;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;
using Xunit;

namespace lemonSpire2.Tests.PlayerStateEx;

public sealed class RemoteUiFlashSnapshotMatcherTests
{
    [Fact]
    public void MatchesCard_ShouldMatch_OnIdUpgradeAndEnchantment()
    {
        var expected = CreateCard("STRIKE", 1, "GLOWING");
        var actual = CreateCard("STRIKE", 1, "GLOWING");

        Assert.True(RemoteUiFlashSnapshotMatcher.MatchesCard(expected, actual));
    }

    [Fact]
    public void MatchesCard_ShouldIgnoreProps_WhenNoRelevantPropsProvided()
    {
        var expected = CreateCard("STRIKE", props: CreateIntProps(("Damage", 9)));
        var actual = CreateCard("STRIKE", props: CreateIntProps(("Damage", 20)));

        Assert.True(RemoteUiFlashSnapshotMatcher.MatchesCard(expected, actual));
    }

    [Fact]
    public void MatchesCard_ShouldCompareRelevantProps_WhenRequested()
    {
        var expected = CreateCard("STRIKE", props: CreateIntProps(("Damage", 9), ("Block", 3)));
        var actual = CreateCard("STRIKE", props: CreateIntProps(("Damage", 12), ("Block", 3)));

        Assert.False(RemoteUiFlashSnapshotMatcher.MatchesCard(expected, actual, ["Damage"]));
        Assert.True(RemoteUiFlashSnapshotMatcher.MatchesCard(expected, actual, ["Block"]));
    }

    [Fact]
    public void MatchesPotionSlot_ShouldRequireSameIdAndSlot()
    {
        var expected = new SerializablePotion { Id = new ModelId("POTION", "FIRE"), SlotIndex = 2 };
        var actual = new SerializablePotion { Id = new ModelId("POTION", "FIRE"), SlotIndex = 2 };
        var wrongSlot = new SerializablePotion { Id = new ModelId("POTION", "FIRE"), SlotIndex = 1 };

        Assert.True(RemoteUiFlashSnapshotMatcher.MatchesPotionSlot(expected, actual));
        Assert.False(RemoteUiFlashSnapshotMatcher.MatchesPotionSlot(expected, wrongSlot));
    }

    [Fact]
    public void MatchesPotionId_ShouldOnlyCheckId()
    {
        var expected = new SerializablePotion { Id = new ModelId("POTION", "FIRE"), SlotIndex = 2 };
        var actual = new SerializablePotion { Id = new ModelId("POTION", "FIRE"), SlotIndex = 0 };
        var other = new SerializablePotion { Id = new ModelId("POTION", "ICE"), SlotIndex = 2 };

        Assert.True(RemoteUiFlashSnapshotMatcher.MatchesPotionId(expected, actual));
        Assert.False(RemoteUiFlashSnapshotMatcher.MatchesPotionId(expected, other));
    }

    [Fact]
    public void MatchesRelic_ShouldCompareRelevantProps_WhenProvided()
    {
        var expected = new SerializableRelic
        {
            Id = new ModelId("RELIC", "BAG"),
            Props = CreateIntProps(("Counter", 2), ("Unused", 1))
        };
        var actual = new SerializableRelic
        {
            Id = new ModelId("RELIC", "BAG"),
            Props = CreateIntProps(("Counter", 2), ("Unused", 99))
        };

        Assert.True(RemoteUiFlashSnapshotMatcher.MatchesRelic(expected, actual, ["Counter"]));
        Assert.False(RemoteUiFlashSnapshotMatcher.MatchesRelic(expected, actual, ["Unused"]));
    }

    private static SerializableCard CreateCard(
        string entry,
        int upgradeLevel = 0,
        string? enchantmentId = null,
        SavedProperties? props = null)
    {
        return new SerializableCard
        {
            Id = new ModelId("CARD", entry),
            CurrentUpgradeLevel = upgradeLevel,
            Enchantment = enchantmentId == null
                ? null
                : new SerializableEnchantment
                {
                    Id = new ModelId("ENCHANTMENT", enchantmentId),
                    Amount = 1
                },
            Props = props
        };
    }

    private static SavedProperties CreateIntProps(params (string Name, int Value)[] values)
    {
        var props = new SavedProperties
        {
            ints = new List<SavedProperties.SavedProperty<int>>()
        };

        foreach (var (name, value) in values)
            props.ints.Add(new SavedProperties.SavedProperty<int>(name, value));

        return props;
    }
}
