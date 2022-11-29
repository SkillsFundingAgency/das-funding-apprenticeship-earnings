namespace SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Queries
{
    public interface IQueryDispatcher
    {
        Task<TResult> Send<TQuery, TResult>(TQuery query) where TQuery : IQuery;
    }
}
