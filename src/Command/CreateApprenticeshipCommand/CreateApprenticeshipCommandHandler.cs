using NServiceBus;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Factories;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.CreateApprenticeshipCommand
{
    public class CreateApprenticeshipCommandHandler : ICreateApprenticeshipCommandHandler
    {
        private readonly IApprenticeshipFactory _apprenticeshipFactory;
        private readonly IMessageSession _messageSession;
        private readonly IEarningsGeneratedEventBuilder _earningsGeneratedEventBuilder;

        public CreateApprenticeshipCommandHandler(IApprenticeshipFactory apprenticeshipFactory, IMessageSession messageSession, IEarningsGeneratedEventBuilder earningsGeneratedEventBuilder)
        {
            _apprenticeshipFactory = apprenticeshipFactory;
            _messageSession = messageSession;
            _earningsGeneratedEventBuilder = earningsGeneratedEventBuilder;
        }

        public async Task<Apprenticeship> Handle(CreateApprenticeshipCommand command)
        {
            var apprenticeship = _apprenticeshipFactory.CreateNew(command.ApprenticeshipEntity);
            apprenticeship.CalculateEarnings();
            await _messageSession.Publish(_earningsGeneratedEventBuilder.Build(apprenticeship));
            return apprenticeship;
        }
    }
}
