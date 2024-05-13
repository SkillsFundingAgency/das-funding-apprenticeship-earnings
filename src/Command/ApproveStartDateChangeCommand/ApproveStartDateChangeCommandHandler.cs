using NServiceBus;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.ApproveStartDateChangeCommand;

public class ApproveStartDateChangeCommandHandler : IApproveStartDateChangeCommandHandler
{
    private readonly IMessageSession _messageSession;
    private readonly IApprenticeshipEarningsRecalculatedEventBuilder _eventBuilder;

    public ApproveStartDateChangeCommandHandler(IMessageSession messageSession, IApprenticeshipEarningsRecalculatedEventBuilder eventBuilder)
    {
        _messageSession = messageSession;
        _eventBuilder = eventBuilder;
    }

    public async Task<Apprenticeship> RecalculateEarnings(ApproveStartDateChangeCommand command)
    {
        var apprenticeshipDomainModel = GetDomainModel(command.ApprenticeshipEntity);
        
        var newStartDate = command.ApprenticeshipStartDateChangedEvent.ActualStartDate;
        apprenticeshipDomainModel.RecalculateEarnings(newStartDate);
        
        await _messageSession.Publish(_eventBuilder.Build(apprenticeshipDomainModel));
        
        return apprenticeshipDomainModel;
    }

    //TODO two methods below should be in a shared place (currently duplicated)
    private static Apprenticeship GetDomainModel(ApprenticeshipEntityModel entityModel)
    {
        return new Apprenticeship(
            entityModel.ApprenticeshipKey,
            entityModel.ApprovalsApprenticeshipId,
            entityModel.Uln,
            entityModel.UKPRN,
            entityModel.EmployerAccountId,
            entityModel.LegalEntityName,
            entityModel.ActualStartDate,
            entityModel.PlannedEndDate,
            entityModel.AgreedPrice,
            entityModel.TrainingCode,
            entityModel.FundingEmployerAccountId,
            entityModel.FundingType,
            entityModel.FundingBandMaximum,
            entityModel.AgeAtStartOfApprenticeship,
            MapModelToEarningsProfile(entityModel.EarningsProfile)
        );
    }

    private static EarningsProfile MapModelToEarningsProfile(EarningsProfileEntityModel model)
    {
        var instalments = model.Instalments.Select(x => new Instalment(x.AcademicYear, x.DeliveryPeriod, x.Amount)).ToList();
        return new EarningsProfile(model.AdjustedPrice, instalments, model.CompletionPayment, model.EarningsProfileId);
    }

}
