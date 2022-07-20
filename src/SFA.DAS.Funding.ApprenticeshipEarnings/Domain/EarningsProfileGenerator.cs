using NServiceBus;
using SFA.DAS.Apprenticeships.Events;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain
{
    public interface IEarningsProfileGenerator
    {
        public Task<EarningsProfile> GenerateEarnings(ApprenticeshipCreatedEvent apprenticeshipLearnerEvent);
    }

    public class EarningsProfileGenerator : IEarningsProfileGenerator
    {
        private readonly IAdjustedPriceProcessor _adjustedPriceProcessor;
        private readonly IInstallmentsGenerator _installmentsGenerator;
        private readonly IMessageSession _messageSession;
        private readonly IEarningsGeneratedEventBuilder _earningsGeneratedEventBuilder;

        public EarningsProfileGenerator(
            IAdjustedPriceProcessor adjustedPriceProcessor,
            IInstallmentsGenerator installmentsGenerator,
            IMessageSession messageSession,
            IEarningsGeneratedEventBuilder earningsGeneratedEventBuilder)
        {
            _adjustedPriceProcessor = adjustedPriceProcessor;
            _installmentsGenerator = installmentsGenerator;
            _messageSession = messageSession;
            _earningsGeneratedEventBuilder = earningsGeneratedEventBuilder;
        }

        public async Task<EarningsProfile> GenerateEarnings(ApprenticeshipCreatedEvent apprenticeshipLearnerEvent)
        {
            var earningsProfile = new EarningsProfile { AdjustedPrice = _adjustedPriceProcessor.CalculateAdjustedPrice(apprenticeshipLearnerEvent.AgreedPrice) };
            earningsProfile.Installments = _installmentsGenerator.Generate(earningsProfile.AdjustedPrice.Value, apprenticeshipLearnerEvent.ActualStartDate.GetValueOrDefault(), apprenticeshipLearnerEvent.PlannedEndDate.GetValueOrDefault());

            await _messageSession.Publish(_earningsGeneratedEventBuilder.Build(apprenticeshipLearnerEvent, earningsProfile));

            return earningsProfile;
        }
    }
}
