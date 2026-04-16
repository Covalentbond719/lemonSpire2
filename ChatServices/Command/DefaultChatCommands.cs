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
            Description = ChatCmdText.AboutDescription(),
            Args = [],
            Execute = _ => new LocalDisplayChatCmdResult(ChatCmdText.AboutBody(), ChatCmdText.SystemHeader())
        });

        registry.Register(new ChatCmdSpec
        {
            Name = "ping",
            Description = ChatCmdText.PingDescription(),
            Args = [],
            Execute = _ => new LocalDisplayChatCmdResult(ChatCmdText.PingResponse(), ChatCmdText.SystemHeader())
        });

        registry.Register(new ChatCmdSpec
        {
            Name = "help",
            Description = ChatCmdText.HelpDescription(),
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
                    var lines = registry.All.Select(command =>
                        ChatCmdText.HelpListEntry(command.Usage, command.Description));
                    return new LocalDisplayChatCmdResult(string.Join('\n', lines), ChatCmdText.SystemHeader());
                }

                registry.TryGet(commandName, out var command);
                return command is null
                    ? new ErrorChatCmdResult(ChatCmdText.UnknownCommand(commandName), ChatCmdText.SystemHeader())
                    : new LocalDisplayChatCmdResult($"{command.Usage}\n{command.Description}",
                        ChatCmdText.SystemHeader());
            }
        });

        registry.Register(new ChatCmdSpec
        {
            Name = "w",
            Description = ChatCmdText.WhisperDescription(),
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
