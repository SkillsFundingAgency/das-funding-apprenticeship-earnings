using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.PriceChangeApprovedCommand;

public class PriceChangeApprovedCommand
{
    public PriceChangeApprovedCommand(ApprenticeshipEntityModel apprenticeshipEntity, PriceChangeApprovedEvent priceChangeApprovedEvent)
    {
        ApprenticeshipEntity = apprenticeshipEntity;
        PriceChangeApprovedEvent = priceChangeApprovedEvent;    
    }

    public ApprenticeshipEntityModel ApprenticeshipEntity { get; }
    public PriceChangeApprovedEvent PriceChangeApprovedEvent { get; }

}