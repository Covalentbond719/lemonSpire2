using lemonSpire2.Chat.Input.Command;
using lemonSpire2.Chat.Input.Model;

namespace lemonSpire2.Chat.Input.Arguments;

public sealed class PlayerMentionChatArgumentType(Func<IReadOnlyList<MentionTarget>> getMentionTargets)
    : IChatArgumentType
{
    public string DisplayName => "player";

    public bool TryParse(string token, out object? value, out string? error)
    {
        ArgumentNullException.ThrowIfNull(token);
        value = null;
        error = null;

        if (!token.StartsWith('@'))
        {
            error = ChatCmdText.PlayerArgumentMustUseMention();
            return false;
        }

        var alias = token[1..];
        var matches = getMentionTargets()
            .Where(target => string.Equals(target.MentionText, alias, StringComparison.Ordinal))
            .Take(2)
            .ToArray();
        if (matches.Length != 1)
        {
            error = ChatCmdText.UnknownPlayer(alias);
            return false;
        }

        value = matches[0];
        return true;
    }

    public IReadOnlyList<ChatCompletionItem> GetCompletions(string partialToken)
    {
        var query = partialToken.TrimStart('@');
        return
        [
            .. getMentionTargets()
                .Where(target =>
                    target.DisplayName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    target.MentionText.Contains(query, StringComparison.OrdinalIgnoreCase))
                .Select(target => new ChatCompletionItem(target.GetDisplayText(), $"@{target.MentionText} "))
        ];
    }
}
