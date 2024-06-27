using Microsoft.Extensions.Internal;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

public class ApprenticeshipEpisode
{
    public long UKPRN { get; }
    public long EmployerAccountId { get; }
    public DateTime ActualStartDate { get; }
    public DateTime PlannedEndDate { get; private set; }
    public decimal AgreedPrice { get; private set; }

    public ApprenticeshipEpisode(ApprenticeshipEpisodeModel apprenticeshipEpisodeModel)
    {
        UKPRN = apprenticeshipEpisodeModel.UKPRN;
        EmployerAccountId = apprenticeshipEpisodeModel.EmployerAccountId;
        ActualStartDate = apprenticeshipEpisodeModel.ActualStartDate;
        PlannedEndDate = apprenticeshipEpisodeModel.PlannedEndDate;
        AgreedPrice = apprenticeshipEpisodeModel.AgreedPrice;
    }

    public void UpdateAgreedPrice(ISystemClock systemClock, decimal agreedPrice)
    {
        AgreedPrice = agreedPrice;
        // PlannedEndDate = systemClock.UtcNow.DateTime; // TO BE COMPLETED IN SUBTASK FLP-800
    }
}