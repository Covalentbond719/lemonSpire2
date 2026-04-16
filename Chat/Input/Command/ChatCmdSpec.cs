namespace lemonSpire2.Chat.Input.Command;

public sealed class ChatCmdSpec
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required IReadOnlyList<ChatCmdArgSpec> Args { get; init; }
    public required Func<ChatCmdInvocation, ChatCmdResult> Execute { get; init; }

    public string Usage
    {
        get
        {
            var args = string.Join(" ", Args.Select(FormatArg));
            return string.IsNullOrEmpty(args) ? $"/{Name}" : $"/{Name} {args}";
        }
    }

    private static string FormatArg(ChatCmdArgSpec arg)
    {
        var open = arg.IsOptional ? "[" : "<";
        var close = arg.IsOptional ? "]" : ">";
        var suffix = arg.IsGreedy ? "..." : string.Empty;
        return $"{open}{arg.Name}:{arg.ArgType.DisplayName}{suffix}{close}";
    }
}
