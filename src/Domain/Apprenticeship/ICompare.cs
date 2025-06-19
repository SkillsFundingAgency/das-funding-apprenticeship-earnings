namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

public interface ICompare<T>
{
    public bool AreSame(T? compare);
}
