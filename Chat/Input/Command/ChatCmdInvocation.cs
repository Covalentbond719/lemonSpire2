namespace lemonSpire2.Chat.Input.Command;

public sealed class ChatCmdInvocation
{
    public required ChatCmdSpec Command { get; init; }
    public required IReadOnlyList<object?> Arguments { get; init; }

    public T Get<T>(int index)
    {
        return (T)Arguments[index]!;
    }
}
