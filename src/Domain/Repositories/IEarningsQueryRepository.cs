namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories
{
    public interface IEarningsQueryRepository
    {
        Task Add(Apprenticeship.Apprenticeship apprenticeship);
    }
}
