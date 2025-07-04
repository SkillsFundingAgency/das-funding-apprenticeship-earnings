﻿using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship.Events;

// This event is fired in process and the inner event is fired out of process to Azure Service Bus.

// Inprocess event
public class EarningsProfileUpdatedEvent : IDomainEvent
{
    // Azure out of process event
    public ArchiveEarningsProfileEvent ArchiveEarningsProfileEvent { get; }

    public EarningsProfileUpdatedEvent(ArchiveEarningsProfileEvent archiveEarningsProfileEvent)
    {
        ArchiveEarningsProfileEvent = archiveEarningsProfileEvent;
    }
}
