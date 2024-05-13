using NServiceBus;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.ApprovePriceChangeCommand;

public class ApprovePriceChangeCommandHandler : IApprovePriceChangeCommandHandler
{
    private readonly IMessageSession _messageSession;
    private readonly IApprenticeshipEarningsRecalculatedEventBuilder _eventBuilder;

    public ApprovePriceChangeCommandHandler(IMessageSession messageSession, IApprenticeshipEarningsRecalculatedEventBuilder eventBuilder)
    {
        _messageSession = messageSession;
        _eventBuilder = eventBuilder;
    }

    public async Task<Apprenticeship> RecalculateEarnings(ApprovePriceChangeCommand command)
    {
        var apprenticeship = Parse(command);
        var agreedPrice = command.PriceChangeApprovedEvent.AssessmentPrice + command.PriceChangeApprovedEvent.TrainingPrice;
        apprenticeship.RecalculateEarnings(agreedPrice, command.PriceChangeApprovedEvent.EffectiveFromDate);
        await _messageSession.Publish(_eventBuilder.Build(apprenticeship));
        return apprenticeship;
    }

    private static Apprenticeship Parse(ApprovePriceChangeCommand command)
    {
        return new Apprenticeship(
            command.ApprenticeshipEntity.ApprenticeshipKey,
            command.ApprenticeshipEntity.ApprovalsApprenticeshipId,
            command.ApprenticeshipEntity.Uln,
            command.ApprenticeshipEntity.UKPRN,
            command.ApprenticeshipEntity.EmployerAccountId,
            command.ApprenticeshipEntity.LegalEntityName,
            command.ApprenticeshipEntity.ActualStartDate,
            command.ApprenticeshipEntity.PlannedEndDate,
            command.ApprenticeshipEntity.AgreedPrice,
            command.ApprenticeshipEntity.TrainingCode,
            command.ApprenticeshipEntity.FundingEmployerAccountId,
            command.ApprenticeshipEntity.FundingType,
            command.ApprenticeshipEntity.FundingBandMaximum,
            command.ApprenticeshipEntity.AgeAtStartOfApprenticeship,
            MapModelToEarningsProfile(command.ApprenticeshipEntity.EarningsProfile)
        );
    }

    private static EarningsProfile MapModelToEarningsProfile(EarningsProfileEntityModel model)
    {
        var installments = model.Instalments.Select(x => new Instalment(x.AcademicYear, x.DeliveryPeriod, x.Amount)).ToList();
        return new EarningsProfile(model.AdjustedPrice, installments, model.CompletionPayment, model.EarningsProfileId);
    }

}
