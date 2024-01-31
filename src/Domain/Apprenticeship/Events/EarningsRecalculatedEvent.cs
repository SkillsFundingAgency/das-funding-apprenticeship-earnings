namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship.Events
{
    public class EarningsRecalculatedEvent : IDomainEvent
    {
        public Apprenticeship Apprenticeship { get; }

        public EarningsRecalculatedEvent(Apprenticeship apprenticeship)
        {
            Apprenticeship = apprenticeship;
        }
    }
}
