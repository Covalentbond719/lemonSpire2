using lemonSpire2.Chat.Input.Arguments;
using lemonSpire2.Chat.Input.Command;
using lemonSpire2.Chat.Input.Model;
using lemonSpire2.Chat.Message;

namespace lemonSpire2.Chat.Input.Service.Command;

public static class DefaultChatCmds
{
    public static void RegisterDefaults(
        ChatCmdRegistry registry,
        Func<IReadOnlyList<MentionTarget>> getMentionTargets,
        Func<ulong> getLocalNetId,
        Func<string, IReadOnlyCollection<IMsgSegment>> parseMessage)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(getMentionTargets);
        ArgumentNullException.ThrowIfNull(getLocalNetId);
        ArgumentNullException.ThrowIfNull(parseMessage);

        var commandNameType = new CommandNameChatArgumentType(registry);

        registry.Register(new ChatCmdSpec
        {
            Name = "about",
            Description = "Show information about lemonSpire2.",
            Args = [],
            Execute = _ => new LocalDisplayChatCmdResult(
                """
                lemonSpire2 is a mod for Chat Panel, item send and other awesome multiplayer communication features.

                GitHub: https://www.github.com/freude916/lemonSpire2/

                This mod is open-source and free to use.
                If you like it, please consider starring the GitHub repo and sharing it with your friends!
                Please sponsor me! JK... unless? (Wait, I don't even have a Patreon lol)
                """
            )
        });

        registry.Register(new ChatCmdSpec
        {
            Name = "ping",
            Description = "Local connectivity sanity check.",
            Args = [],
            Execute = _ => new LocalDisplayChatCmdResult("pong")
        });

        registry.Register(new ChatCmdSpec
        {
            Name = "help",
            Description = "Show help for chat commands.",
            Args =
            [
                new ChatCmdArgSpec("command", commandNameType, IsOptional: true)
            ],
            Execute = invocation =>
            {
                var commandName = invocation.Get<string?>(0);
                if (string.IsNullOrWhiteSpace(commandName))
                {
                    // 不传命令名时，help 退化成“列出当前注册的所有命令签名”。
                    var lines = registry.All.Select(static command => $"{command.Usage} - {command.Description}");
                    return new LocalDisplayChatCmdResult(string.Join('\n', lines));
                }

                registry.TryGet(commandName, out var command);
                return command is null
                    ? new ErrorChatCmdResult($"Unknown command '{commandName}'.")
                    : new LocalDisplayChatCmdResult($"{command.Usage}\n{command.Description}");
            }
        });

        registry.Register(new ChatCmdSpec
        {
            Name = "w",
            Description = "Whisper to a player.",
            Args =
            [
                new ChatCmdArgSpec("player", new PlayerMentionChatArgumentType(getMentionTargets)),
                new ChatCmdArgSpec("message", new GreedyStringChatArgumentType(), IsGreedy: true)
            ],
            Execute = invocation =>
            {
                var target = invocation.Get<MentionTarget>(0);
                var message = invocation.Get<string>(1);
                // /w 仍然复用普通聊天 parser，把富文本 / mention / inline-ref 继续转成标准 segments。
                var parsedMessage = parseMessage(message);
                var localNetId = getLocalNetId();
                var messages = new List<ChatCmdOutgoingMessage>
                {
                    new(target.PlayerNetId, parsedMessage)
                };

                if (target.PlayerNetId != localNetId)
                    messages.Add(new ChatCmdOutgoingMessage(localNetId, parsedMessage));

                return new SendSegmentsChatCmdResult(messages);
            }
        });
    }
}
