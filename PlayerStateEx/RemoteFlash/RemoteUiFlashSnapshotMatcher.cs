using MegaCrit.Sts2.Core.Saves.Runs;

namespace lemonSpire2.PlayerStateEx.RemoteFlash;

public static class RemoteUiFlashSnapshotMatcher
{
    public static bool MatchesCard(
        SerializableCard? expected,
        SerializableCard? actual,
        IEnumerable<string>? relevantPropNames = null)
    {
        if (expected?.Id == null || actual?.Id == null) return false;
        if (expected.Id != actual.Id) return false;
        if (expected.CurrentUpgradeLevel != actual.CurrentUpgradeLevel) return false;
        if (!Equals(expected.Enchantment, actual.Enchantment)) return false;

        return MatchesSavedProperties(expected.Props, actual.Props, relevantPropNames);
    }

    public static bool MatchesRelic(
        SerializableRelic? expected,
        SerializableRelic? actual,
        IEnumerable<string>? relevantPropNames = null)
    {
        if (expected?.Id == null || actual?.Id == null) return false;
        if (expected.Id != actual.Id) return false;

        return MatchesSavedProperties(expected.Props, actual.Props, relevantPropNames);
    }

    public static bool MatchesPotionSlot(SerializablePotion? expected, SerializablePotion? actual)
    {
        if (expected?.Id == null || actual?.Id == null) return false;
        return expected.Id == actual.Id && expected.SlotIndex == actual.SlotIndex;
    }

    public static bool MatchesPotionId(SerializablePotion? expected, SerializablePotion? actual)
    {
        if (expected?.Id == null || actual?.Id == null) return false;
        return expected.Id == actual.Id;
    }

    private static bool MatchesSavedProperties(
        SavedProperties? expected,
        SavedProperties? actual,
        IEnumerable<string>? relevantPropNames)
    {
        var names = relevantPropNames?.ToArray() ?? [];
        if (names.Length == 0 || expected == null) return true;
        if (actual == null) return false;

        foreach (var name in names)
            if (TryGetSavedProperty(expected, name, out var expectedValue, out var expectedKind))
            {
                if (!TryGetSavedProperty(actual, name, out var actualValue, out var actualKind)) return false;
                if (expectedKind != actualKind) return false;
                if (!AreSavedValuesEqual(expectedValue, actualValue)) return false;
            }

        return true;
    }

    private static bool TryGetSavedProperty(
        SavedProperties props,
        string name,
        out object? value,
        out string kind)
    {
        ArgumentNullException.ThrowIfNull(props);

        foreach (var item in props.ints ?? [])
            if (item.name == name)
            {
                value = item.value;
                kind = "int";
                return true;
            }

        foreach (var item in props.bools ?? [])
            if (item.name == name)
            {
                value = item.value;
                kind = "bool";
                return true;
            }

        foreach (var item in props.strings ?? [])
            if (item.name == name)
            {
                value = item.value;
                kind = "string";
                return true;
            }

        foreach (var item in props.intArrays ?? [])
            if (item.name == name)
            {
                value = item.value;
                kind = "int[]";
                return true;
            }

        foreach (var item in props.modelIds ?? [])
            if (item.name == name)
            {
                value = item.value;
                kind = "modelId";
                return true;
            }

        foreach (var item in props.cards ?? [])
            if (item.name == name)
            {
                value = item.value;
                kind = "card";
                return true;
            }

        foreach (var item in props.cardArrays ?? [])
            if (item.name == name)
            {
                value = item.value;
                kind = "card[]";
                return true;
            }

        value = null;
        kind = "";
        return false;
    }

    private static bool AreSavedValuesEqual(object? expected, object? actual)
    {
        return (expected, actual) switch
        {
            (null, null) => true,
            (int[] left, int[] right) => left.SequenceEqual(right),
            (SerializableCard left, SerializableCard right) => MatchesCard(left, right),
            (SerializableCard[] left, SerializableCard[] right) => left.Length == right.Length &&
                                                                   left.Zip(right).All(pair =>
                                                                       MatchesCard(pair.First, pair.Second)),
            _ => Equals(expected, actual)
        };
    }
}
