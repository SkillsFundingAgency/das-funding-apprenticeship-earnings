using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.ApproveStartDateChangeCommand;

public interface IApproveStartDateChangeCommandHandler
{
    Task<Apprenticeship> RecalculateEarnings(ApproveStartDateChangeCommand command);
}
