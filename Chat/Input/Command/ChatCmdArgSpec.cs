using lemonSpire2.Chat.Input.Arguments;

namespace lemonSpire2.Chat.Input.Command;

public sealed record ChatCmdArgSpec(
    string Name,
    IChatArgumentType ArgType,
    bool IsOptional = false,
    bool IsGreedy = false);
