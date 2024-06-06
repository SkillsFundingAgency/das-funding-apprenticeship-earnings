using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.ApprovePriceChangeCommand;

public class ApprovePriceChangeCommand
{
    public ApprovePriceChangeCommand(ApprenticeshipEntityModel apprenticeshipEntity, PriceChangeApprovedEvent priceChangeApprovedEvent)
    {
        ApprenticeshipEntity = apprenticeshipEntity;
        PriceChangeApprovedEvent = priceChangeApprovedEvent;    
    }

    public ApprenticeshipEntityModel ApprenticeshipEntity { get; }
    public PriceChangeApprovedEvent PriceChangeApprovedEvent { get; }
}