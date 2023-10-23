using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.PriceChangeApprovedCommand;

public class PriceChangeApprovedCommand
{
    public PriceChangeApprovedCommand(ApprenticeshipEntityModel apprenticeshipEntity, PriceChangeDetails priceChangeDetails)
    {
        ApprenticeshipEntity = apprenticeshipEntity;
        PriceChangeDetails = priceChangeDetails;    
    }

    public ApprenticeshipEntityModel ApprenticeshipEntity { get; }
    public PriceChangeDetails PriceChangeDetails { get; }

}


public class PriceChangeDetails
{
    public long ApprenticeshipId { get; set; }

    public decimal TrainingPrice { get; set; }

    public decimal AssessmentPrice { get; set; }
    public DateTime EffectiveFromDate { get; set; }

    public DateTime ApprovedDate { get; set; }
    public ApprovedBy ApprovedBy { get; set; }

    public long EmployerAccountId { get; set; }

    public long ProviderId { get; set; }

}