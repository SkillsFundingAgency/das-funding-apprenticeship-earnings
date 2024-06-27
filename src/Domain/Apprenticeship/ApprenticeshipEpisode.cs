using Microsoft.Extensions.Internal;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

public class ApprenticeshipEpisode
{
    public long UKPRN { get; }
    public long EmployerAccountId { get; }
    public DateTime ActualStartDate { get; private set; }
    public DateTime PlannedEndDate { get; private set; }
    public decimal AgreedPrice { get; private set; }
    public string TrainingCode { get; }
    public FundingType FundingType { get; }
    public decimal FundingBandMaximum { get; }
    public string LegalEntityName { get; }

    public ApprenticeshipEpisode(ApprenticeshipEpisodeModel apprenticeshipEpisodeModel)
    {
        UKPRN = apprenticeshipEpisodeModel.UKPRN;
        EmployerAccountId = apprenticeshipEpisodeModel.EmployerAccountId;
        ActualStartDate = apprenticeshipEpisodeModel.ActualStartDate;
        PlannedEndDate = apprenticeshipEpisodeModel.PlannedEndDate;
        AgreedPrice = apprenticeshipEpisodeModel.AgreedPrice;
        TrainingCode = apprenticeshipEpisodeModel.TrainingCode;
        FundingType = apprenticeshipEpisodeModel.FundingType;
        FundingBandMaximum = apprenticeshipEpisodeModel.FundingBandMaximum;
        LegalEntityName = apprenticeshipEpisodeModel.LegalEntityName;
    }

    public void UpdateAgreedPrice(ISystemClock systemClock, decimal agreedPrice)
    {
        AgreedPrice = agreedPrice;
        // PlannedEndDate = systemClock.UtcNow.DateTime; // TO BE COMPLETED IN SUBTASK FLP-800
    }

    public void UpdateStartDate(DateTime startDate, DateTime endDate) 
    { 
        ActualStartDate = startDate;
        PlannedEndDate = endDate;
        // THIS HANDLING MAY NEED TO BE REFINED IN SUBTASK FLP-801
    }
}