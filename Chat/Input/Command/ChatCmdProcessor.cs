using lemonSpire2.Chat.Input.Parsing;

namespace lemonSpire2.Chat.Input.Command;

public sealed class ChatCmdProcessor(ChatCmdRegistry registry)
{
    public ChatCmdResult Process(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        if (!text.StartsWith('/'))
            return new NotAChatCmdResult();

        var tokenizeResult = ChatCmdTokenizer.Tokenize(text);
        if (tokenizeResult.Tokens.Count == 0)
            return new ErrorChatCmdResult(ChatCmdText.CommandNameRequired(), ChatCmdText.SystemHeader());

        var commandName = tokenizeResult.Tokens[0].Value;
        if (!registry.TryGet(commandName, out var command) || command is null)
            return new ErrorChatCmdResult(ChatCmdText.UnknownCommand(commandName), ChatCmdText.SystemHeader());

        var rawArgs = tokenizeResult.Tokens.Skip(1).ToArray();
        var parsedArgs = new List<object?>(command.Args.Count);

        for (var argIndex = 0; argIndex < command.Args.Count; argIndex++)
        {
            var spec = command.Args[argIndex];
            if (spec.IsGreedy)
            {
                // Greedy 参数吞掉当前位置之后的全部 token，因此它必须是签名中的最后一个参数。
                if (argIndex >= rawArgs.Length)
                {
                    if (!spec.IsOptional)
                        return new ErrorChatCmdResult(ChatCmdText.MissingArgument(spec.Name),
                            ChatCmdText.SystemHeader());

                    parsedArgs.Add(null);
                    continue;
                }

                var greedyValue = string.Join(" ", rawArgs.Skip(argIndex).Select(static token => token.Value));
                if (!spec.ArgType.TryParse(greedyValue, out var parsedValue, out var parseError))
                    return new ErrorChatCmdResult(parseError ?? ChatCmdText.InvalidArgument(spec.Name),
                        ChatCmdText.SystemHeader());

                parsedArgs.Add(parsedValue);
                if (argIndex + 1 != command.Args.Count)
                    return new ErrorChatCmdResult(ChatCmdText.GreedyArgumentMustBeFinal(spec.Name),
                        ChatCmdText.SystemHeader());

                return command.Execute(new ChatCmdInvocation
                {
                    Command = command,
                    Arguments = parsedArgs
                });
            }

            if (argIndex >= rawArgs.Length)
            {
                if (!spec.IsOptional)
                    return new ErrorChatCmdResult(ChatCmdText.MissingArgument(spec.Name), ChatCmdText.SystemHeader());

                parsedArgs.Add(null);
                continue;
            }

            if (!spec.ArgType.TryParse(rawArgs[argIndex].Value, out var value, out var error))
                return new ErrorChatCmdResult(error ?? ChatCmdText.InvalidArgument(spec.Name),
                    ChatCmdText.SystemHeader());

            parsedArgs.Add(value);
        }

        // 固定签名模式下，遍历完声明参数后还剩 token，就只能视为参数过多。
        if (rawArgs.Length > command.Args.Count)
            return new ErrorChatCmdResult(ChatCmdText.TooManyArguments(command.Name), ChatCmdText.SystemHeader());

        return command.Execute(new ChatCmdInvocation
        {
            Command = command,
            Arguments = parsedArgs
        });
    }
}
