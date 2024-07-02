using Microsoft.Extensions.Internal;
using NServiceBus;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Factories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.CreateApprenticeshipCommand
{
    public class CreateApprenticeshipCommandHandler : ICreateApprenticeshipCommandHandler
    {
        private readonly IApprenticeshipFactory _apprenticeshipFactory;
        private readonly IMessageSession _messageSession;
        private readonly IEarningsGeneratedEventBuilder _earningsGeneratedEventBuilder; 
        private readonly ISystemClockService _systemClock;

        public CreateApprenticeshipCommandHandler(
            IApprenticeshipFactory apprenticeshipFactory, IMessageSession messageSession, IEarningsGeneratedEventBuilder earningsGeneratedEventBuilder, ISystemClockService systemClock)
        {
            _apprenticeshipFactory = apprenticeshipFactory;
            _messageSession = messageSession;
            _earningsGeneratedEventBuilder = earningsGeneratedEventBuilder;
            _systemClock = systemClock;
        }

        public async Task<Apprenticeship> Create(CreateApprenticeshipCommand command)
        {
            var apprenticeship = _apprenticeshipFactory.CreateNew(command.ApprenticeshipEntity);
            apprenticeship.CalculateEarnings(_systemClock);
            await _messageSession.Publish(_earningsGeneratedEventBuilder.Build(apprenticeship));
            return apprenticeship;
        }
    }
}
