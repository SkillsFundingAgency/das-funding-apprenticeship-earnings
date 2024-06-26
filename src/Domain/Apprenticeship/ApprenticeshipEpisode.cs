using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

public class ApprenticeshipEpisode
{
    public long UKPRN { get; }
    public long EmployerAccountId { get; }
    public DateTime ActualStartDate { get; private set; }
    public DateTime PlannedEndDate { get; private set; }

    public ApprenticeshipEpisode(ApprenticeshipEpisodeModel apprenticeshipEpisodeModel)
    {
        UKPRN = apprenticeshipEpisodeModel.UKPRN;
        EmployerAccountId = apprenticeshipEpisodeModel.EmployerAccountId;
        ActualStartDate = apprenticeshipEpisodeModel.ActualStartDate;
        PlannedEndDate = apprenticeshipEpisodeModel.PlannedEndDate;
    }
}