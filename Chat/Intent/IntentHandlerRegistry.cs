using Logger = MegaCrit.Sts2.Core.Logging.Logger;

namespace lemonSpire2.Chat.Intent;

public class IntentHandlerRegistry
{
    private readonly Dictionary<Type, List<Func<IIntent, bool>>> _handlers = new();
    private static Logger Log => ChatUiPatch.Log;

    public void Register<T>(Func<T, bool> handler) where T : IIntent
    {
        Log.Info($"Registering handler for {typeof(T)}");

        if (!_handlers.TryGetValue(typeof(T), out var handlers))
        {
            handlers = [];
            _handlers[typeof(T)] = handlers;
        }

        handlers.Add(intent => handler((T)intent));
    }

    public bool TryHandle(IIntent intent)
    {
        ArgumentNullException.ThrowIfNull(intent);
        if (_handlers.TryGetValue(intent.GetType(), out var handlers))
        {
            Log.Info($"Handling handler for {intent.GetType()}");

            var handled = false;
            foreach (var handler in handlers)
                handled |= handler(intent);

            return handled;
        }

        Log.Debug($"No handler registered for {intent.GetType()}");
        return false;
    }
}
