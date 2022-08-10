using NServiceBus;
using SFA.DAS.Apprenticeships.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain
{
    public interface IEarningsProfileGenerator
    {
        public Task<EarningsProfile> GenerateEarnings(ApprenticeshipCreatedEvent apprenticeshipLearnerEvent);
    }

    public class EarningsProfileGenerator : IEarningsProfileGenerator
    {
        private readonly IOnProgramTotalPriceCalculator _onProgramTotalPriceCalculator;
        private readonly IInstallmentsGenerator _installmentsGenerator;
        private readonly IMessageSession _messageSession;
        private readonly IEarningsGeneratedEventBuilder _earningsGeneratedEventBuilder;
        private readonly IAdjustedPriceCalculator _adjustedPriceCalculator;

        public EarningsProfileGenerator(IOnProgramTotalPriceCalculator onProgramTotalPriceCalculator,
            IInstallmentsGenerator installmentsGenerator,
            IMessageSession messageSession,
            IEarningsGeneratedEventBuilder earningsGeneratedEventBuilder,
            IAdjustedPriceCalculator adjustedPriceCalculator)
        {
            _onProgramTotalPriceCalculator = onProgramTotalPriceCalculator;
            _installmentsGenerator = installmentsGenerator;
            _messageSession = messageSession;
            _earningsGeneratedEventBuilder = earningsGeneratedEventBuilder;
            _adjustedPriceCalculator = adjustedPriceCalculator;
        }

        public async Task<EarningsProfile> GenerateEarnings(ApprenticeshipCreatedEvent apprenticeshipLearnerEvent)
        {
            var earningsProfile = new EarningsProfile { OnProgramTotalPrice = _onProgramTotalPriceCalculator.CalculateOnProgramTotalPrice(_adjustedPriceCalculator.CalculateAdjustedPrice(apprenticeshipLearnerEvent)) };
            earningsProfile.Installments = _installmentsGenerator.Generate(earningsProfile.OnProgramTotalPrice.Value, apprenticeshipLearnerEvent.ActualStartDate.GetValueOrDefault(), apprenticeshipLearnerEvent.PlannedEndDate.GetValueOrDefault());

            await _messageSession.Publish(_earningsGeneratedEventBuilder.Build(apprenticeshipLearnerEvent, earningsProfile));

            return earningsProfile;
        }
    }
}
