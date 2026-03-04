using System.Text.Json;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Factories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Services;
using SFA.DAS.Learning.Types;
using LearningDomainModel = SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Learning;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.CreateApprenticeshipCommand
{
    public class CreateApprenticeshipCommandHandler : ICommandHandler<CreateApprenticeshipCommand, LearningDomainModel>
    {
        private readonly ILearningFactory _learningFactory;
        private readonly ILearningRepository _learningRepository;
        private readonly IMessageSession _messageSession;
        private readonly IEarningsGeneratedEventBuilder _earningsGeneratedEventBuilder; 
        private readonly ISystemClockService _systemClock;
        private readonly IFundingBandMaximumService _fundingBandMaximumService;

        public CreateApprenticeshipCommandHandler(
            ILearningFactory learningFactory, 
            ILearningRepository learningRepository, 
            IMessageSession messageSession,
            IEarningsGeneratedEventBuilder earningsGeneratedEventBuilder, 
            ISystemClockService systemClock,
            IFundingBandMaximumService fundingBandMaximumService
            )
        {
            _learningFactory = learningFactory;
            _learningRepository = learningRepository;
            _messageSession = messageSession;
            _earningsGeneratedEventBuilder = earningsGeneratedEventBuilder;
            _systemClock = systemClock;
            _fundingBandMaximumService = fundingBandMaximumService;
        }

        public async Task<LearningDomainModel> Handle(CreateApprenticeshipCommand command, CancellationToken cancellationToken = default)
        {
            var fundingBandMaximum = await GetFundingBandMaximum(command.LearningCreatedEvent);
            var learning = _learningFactory.CreateNew(command.LearningCreatedEvent, fundingBandMaximum);
            learning.Calculate(_systemClock, JsonSerializer.Serialize(command.LearningCreatedEvent));
            await _learningRepository.Add(learning);
            await _messageSession.Publish(_earningsGeneratedEventBuilder.Build(learning));
            return learning;
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
