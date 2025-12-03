using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Factories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Services;
using SFA.DAS.Learning.Types;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.CreateApprenticeshipCommand
{
    public class CreateApprenticeshipCommandHandler : ICommandHandler<CreateApprenticeshipCommand, Apprenticeship>
    {
        private readonly IApprenticeshipFactory _apprenticeshipFactory;
        private readonly IApprenticeshipRepository _apprenticeshipRepository;
        private readonly IMessageSession _messageSession;
        private readonly IEarningsGeneratedEventBuilder _earningsGeneratedEventBuilder; 
        private readonly ISystemClockService _systemClock;
        private readonly IFundingBandMaximumService _fundingBandMaximumService;

        public CreateApprenticeshipCommandHandler(
            IApprenticeshipFactory apprenticeshipFactory, 
            IApprenticeshipRepository apprenticeshipRepository, 
            IMessageSession messageSession,
            IEarningsGeneratedEventBuilder earningsGeneratedEventBuilder, 
            ISystemClockService systemClock,
            IFundingBandMaximumService fundingBandMaximumService
            )
        {
            _apprenticeshipFactory = apprenticeshipFactory;
            _apprenticeshipRepository = apprenticeshipRepository;
            _messageSession = messageSession;
            _earningsGeneratedEventBuilder = earningsGeneratedEventBuilder;
            _systemClock = systemClock;
            _fundingBandMaximumService = fundingBandMaximumService;
        }

        public async Task<Apprenticeship> Handle(CreateApprenticeshipCommand command, CancellationToken cancellationToken = default)
        {
            var fundingBandMaximum = await GetFundingBandMaximum(command.LearningCreatedEvent);
            var apprenticeship = _apprenticeshipFactory.CreateNew(command.LearningCreatedEvent);
            apprenticeship.Calculate(_systemClock);
            await _apprenticeshipRepository.Add(apprenticeship);
            await _messageSession.Publish(_earningsGeneratedEventBuilder.Build(apprenticeship));
            return apprenticeship;
        }

        private async Task<int> GetFundingBandMaximum(LearningCreatedEvent learningCreatedEvent)
        {

            var startDate = learningCreatedEvent.Episode.Prices.Min(x => x.StartDate);
            var courseCode = learningCreatedEvent.Episode.TrainingCode;


            var fundingBandMaximum = await _fundingBandMaximumService.GetFundingBandMaximum(courseCode, startDate);
            if (fundingBandMaximum == null) throw new Exception($"No funding band maximum found for course {courseCode} for given StartDate {startDate}. LearningKey: {learningCreatedEvent.LearningKey}");

            return fundingBandMaximum.Value;
        }
    }
}
