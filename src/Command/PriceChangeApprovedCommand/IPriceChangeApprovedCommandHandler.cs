using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.PriceChangeApprovedCommand;

public interface IPriceChangeApprovedCommandHandler
{
    Task<Apprenticeship> RecalculateEarnings(PriceChangeApprovedCommand command);
}
