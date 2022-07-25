﻿// ReSharper disable once CheckNamespace

using NServiceBus;

namespace SFA.DAS.Apprenticeships.Events;

public class ApprenticeshipCreatedEvent : IEvent // TODO: Use nuget when available
{
    public Guid ApprenticeshipKey { get; set; }

    public long ApprovalsApprenticeshipId { get; set; }

    public long Uln { get; set; }

    public long UKPRN { get; set; }

    public long EmployerAccountId { get; set; }

    public string LegalEntityName { get; set; }

    public DateTime? ActualStartDate { get; set; }

    public DateTime? PlannedEndDate { get; set; }

    public Decimal AgreedPrice { get; set; }

    public string TrainingCode { get; set; }

    public long? FundingEmployerAccountId { get; set; }

    public FundingType FundingType { get; set; }

}

public enum FundingType
{
    Levy,
    NonLevy,
    Transfer,
}