namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain;

public interface IDomainEntity<T>
{
    public bool AreSame(T? compare);
}
