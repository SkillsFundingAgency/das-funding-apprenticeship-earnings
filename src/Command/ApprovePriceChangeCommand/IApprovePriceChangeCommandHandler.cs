using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.ApprovePriceChangeCommand;

public interface IApprovePriceChangeCommandHandler
{
    Task<Apprenticeship> RecalculateEarnings(ApprovePriceChangeCommand command);
}
