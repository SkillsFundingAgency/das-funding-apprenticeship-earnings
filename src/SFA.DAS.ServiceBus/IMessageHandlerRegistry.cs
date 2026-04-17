namespace SFA.DAS.ServiceBus;

internal interface IMessageHandlerRegistry
{
    MessageHandler Resolve(string typeName);
}
