namespace lemonSpire2.Chat.Input.Service.Mention;

public static class MentionAliasService
{
    public static IReadOnlyDictionary<ulong, string> CreateAliases(IEnumerable<MentionAliasSource> sources)
    {
        ArgumentNullException.ThrowIfNull(sources);
        var sourceArray = sources.ToArray();
        var result = new Dictionary<ulong, string>(sourceArray.Length);

        foreach (var group in sourceArray.GroupBy(static source => source.DisplayName, StringComparer.Ordinal))
        {
            var ordered = group.OrderBy(static source => source.PlayerNetId).ToArray();
            if (ordered.Length == 1)
            {
                result[ordered[0].PlayerNetId] = MentionTextCodec.Encode(ordered[0].DisplayName);
                continue;
            }

            // 重名玩家按稳定的 NetId 顺序编号，保证一局内 alias 可预测、可回解析。
            for (var index = 0; index < ordered.Length; index++)
                result[ordered[index].PlayerNetId] =
                    $"{MentionTextCodec.Encode(ordered[index].DisplayName)}_{index + 1}";
        }

        return result;
    }
}
