using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.CreateApprenticeshipCommand
{
    public interface ICreateApprenticeshipCommandHandler
    {
        Task<Apprenticeship> Handle(CreateApprenticeshipCommand command);
    }
}
