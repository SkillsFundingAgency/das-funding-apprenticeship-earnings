using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.ReverseWithdrawal
{
    public class ReverseWithdrawalCommand(Guid apprenticeshipKey) : ICommand
    {
        public Guid ApprenticeshipKey { get; set; } = apprenticeshipKey;
    }

    public class ReverseWithdrawalCommandHandler : ICommandHandler<ReverseWithdrawalCommand>
    {
        private readonly IApprenticeshipRepository _apprenticeshipRepository;
        private readonly ISystemClockService _systemClock;

        public ReverseWithdrawalCommandHandler(IApprenticeshipRepository apprenticeshipRepository, ISystemClockService systemClock)
        {
            _apprenticeshipRepository = apprenticeshipRepository;
            _systemClock = systemClock;
        }

        public async Task Handle(ReverseWithdrawalCommand command, CancellationToken cancellationToken = default)
        {
            var apprenticeshipDomainModel = await _apprenticeshipRepository.Get(command.ApprenticeshipKey);

            apprenticeshipDomainModel.ReverseWithdrawal(_systemClock);
            apprenticeshipDomainModel.Calculate(_systemClock);

            await _apprenticeshipRepository.Update(apprenticeshipDomainModel);
        }
    }

}
