namespace SFA.DAS.ServiceBus;

internal interface IMessageHandlerRegistry
{
    public IEnumerable<MessageHandler> GetAll();
    public MessageHandler Resolve(string typeName);
}
