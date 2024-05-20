using SFA.DAS.Funding.ApprenticeshipEarnings.Domain;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command;

internal static class ApprenticeshipExtensions
{
    internal static List<DeliveryPeriod>? BuildDeliveryPeriods(this Apprenticeship apprenticeship)
    {
        return apprenticeship.EarningsProfile?.Instalments.Select(instalment => new DeliveryPeriod
        (
            instalment.DeliveryPeriod.ToCalendarMonth(),
            instalment.AcademicYear.ToCalendarYear(instalment.DeliveryPeriod),
            instalment.DeliveryPeriod,
            instalment.AcademicYear,
            instalment.Amount,
            apprenticeship.FundingLineType
        )).ToList();
    }

    internal static Apprenticeship GetDomainModel(this ApprenticeshipEntityModel entityModel)
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

    internal static EarningsProfile MapModelToEarningsProfile(EarningsProfileEntityModel model)
    {
        var instalments = model.Instalments.Select(x => new Instalment(x.AcademicYear, x.DeliveryPeriod, x.Amount)).ToList();
        return new EarningsProfile(model.AdjustedPrice, instalments, model.CompletionPayment, model.EarningsProfileId);
    }
}