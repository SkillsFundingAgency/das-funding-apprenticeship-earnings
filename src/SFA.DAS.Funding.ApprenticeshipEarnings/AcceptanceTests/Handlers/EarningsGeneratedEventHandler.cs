using System.Collections.Concurrent;
using NServiceBus;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Acceptance.Handlers;

public class EarningsGeneratedEventHandler : IHandleMessages<EarningsGeneratedEvent>
{
    public static ConcurrentBag<EarningsGeneratedEvent> ReceivedEvents { get; } = new();

    public Task Handle(EarningsGeneratedEvent message, IMessageHandlerContext context)
    {
        ReceivedEvents.Add(message);
        return Task.CompletedTask;
    }
}