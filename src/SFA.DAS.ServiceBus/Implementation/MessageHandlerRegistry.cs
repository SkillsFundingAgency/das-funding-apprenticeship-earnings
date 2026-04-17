namespace SFA.DAS.ServiceBus.Implementation;

internal class MessageHandlerRegistry : IMessageHandlerRegistry
{

    private readonly IEnumerable<MessageHandler> _messageHandlers;

    public MessageHandlerRegistry()
    {
        _messageHandlers = GetMessageHandlerTypes();
    }

    public MessageHandler Resolve(string typeName)
    {
        var handlerMeta = _messageHandlers
            .FirstOrDefault(x => typeName.Contains(x.HandledEventType.FullName!));

        return handlerMeta;
    }

    internal static IEnumerable<MessageHandler> GetMessageHandlerTypes()
    {
        var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();

        var result = allAssemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.GetInterfaces()
                .Any(interfaceType =>
                    interfaceType.IsGenericType &&
                    interfaceType.GetGenericTypeDefinition() == typeof(IHandleMessages<>)))
            .SelectMany(matchingClass => matchingClass.GetInterfaces(),
                (matchingClass, handlerInterface) => new { matchingClass, handlerInterface })
            .Where(t => t.handlerInterface.IsGenericType &&
                        t.handlerInterface.GetGenericTypeDefinition() == typeof(IHandleMessages<>))
            .Select(t => new MessageHandler
            {
                HandlerType = t.matchingClass,
                HandledEventType = t.handlerInterface.GetGenericArguments()[0]
            }).ToList();

        return result;
    }
}
