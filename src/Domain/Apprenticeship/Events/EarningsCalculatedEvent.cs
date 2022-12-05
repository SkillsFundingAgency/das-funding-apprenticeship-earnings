namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship.Events
{
    public class EarningsCalculatedEvent : IDomainEvent
    {
        public Apprenticeship Apprenticeship { get; }

        public EarningsCalculatedEvent(Apprenticeship apprenticeship)
        {
            Apprenticeship = apprenticeship;
        }
    }
}
