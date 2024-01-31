using NServiceBus;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship.Events;
using System.Collections.Concurrent;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Handlers;

public class EarningsRecalculatedEventHandler : IHandleMessages<EarningsRecalculatedEvent>
{
    public static ConcurrentBag<EarningsRecalculatedEvent> ReceivedEvents { get; } = new();

    public Task Handle(EarningsRecalculatedEvent message, IMessageHandlerContext context)
    {
        ReceivedEvents.Add(message);
        return Task.CompletedTask;
    }
}