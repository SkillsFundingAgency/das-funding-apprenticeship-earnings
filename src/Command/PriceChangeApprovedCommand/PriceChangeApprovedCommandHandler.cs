using NServiceBus;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.PriceChangeApprovedCommand;

public class PriceChangeApprovedCommandHandler : IPriceChangeApprovedCommandHandler
{
    private readonly IMessageSession _messageSession;
    private readonly IApprenticeshipEarningsRecalculatedEventBuilder _eventBuilder;

    public PriceChangeApprovedCommandHandler(IMessageSession messageSession, IApprenticeshipEarningsRecalculatedEventBuilder eventBuilder)
    {
        _messageSession = messageSession;
        _eventBuilder = eventBuilder;
    }

    public async Task<Apprenticeship> RecalculateEarnings(PriceChangeApprovedCommand command)
    {
        var apprenticeship = Parse(command);
        var agreedPrice = command.PriceChangeApprovedEvent.AssessmentPrice + command.PriceChangeApprovedEvent.TrainingPrice;
        apprenticeship.RecalculateEarnings(agreedPrice, command.PriceChangeApprovedEvent.EffectiveFromDate);
        await _messageSession.Publish(_eventBuilder.Build(apprenticeship));
        return apprenticeship;
    }

    private static Apprenticeship Parse(PriceChangeApprovedCommand entityModel)
    {
        var newAgreedPrice = entityModel.PriceChangeApprovedEvent.TrainingPrice + entityModel.PriceChangeApprovedEvent.AssessmentPrice;
        return new Apprenticeship(
            entityModel.ApprenticeshipEntity.ApprenticeshipKey,
            entityModel.ApprenticeshipEntity.ApprovalsApprenticeshipId,
            entityModel.ApprenticeshipEntity.Uln,
            entityModel.ApprenticeshipEntity.UKPRN,
            entityModel.ApprenticeshipEntity.EmployerAccountId,
            entityModel.ApprenticeshipEntity.LegalEntityName,
            entityModel.ApprenticeshipEntity.ActualStartDate,
            entityModel.ApprenticeshipEntity.PlannedEndDate,
            entityModel.ApprenticeshipEntity.AgreedPrice,
            entityModel.ApprenticeshipEntity.TrainingCode,
            entityModel.ApprenticeshipEntity.FundingEmployerAccountId,
            entityModel.ApprenticeshipEntity.FundingType,
            entityModel.ApprenticeshipEntity.FundingBandMaximum,
            entityModel.ApprenticeshipEntity.AgeAtStartOfApprenticeship,
            MapModelToEarningsProfile(entityModel.ApprenticeshipEntity.EarningsProfile)
        );
    }

    private static EarningsProfile MapModelToEarningsProfile(EarningsProfileEntityModel model)
    {
        var instalments = model.Instalments.Select(x => new Instalment(x.AcademicYear, x.DeliveryPeriod, x.Amount)).ToList();
        return new EarningsProfile(model.AdjustedPrice, instalments, model.CompletionPayment, model.EarningsProfileId);
    }

}
