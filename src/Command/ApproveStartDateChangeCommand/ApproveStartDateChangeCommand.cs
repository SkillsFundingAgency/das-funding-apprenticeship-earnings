using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.ApproveStartDateChangeCommand;

public class ApproveStartDateChangeCommand 
{
    public ApproveStartDateChangeCommand(ApprenticeshipEntityModel apprenticeshipEntity, ApprenticeshipStartDateChangedEvent startDateChangedEvent)
    {
        ApprenticeshipEntity = apprenticeshipEntity;
        ApprenticeshipStartDateChangedEvent = startDateChangedEvent;    
    }

    public ApprenticeshipEntityModel ApprenticeshipEntity { get; }
    public ApprenticeshipStartDateChangedEvent ApprenticeshipStartDateChangedEvent { get; }

}