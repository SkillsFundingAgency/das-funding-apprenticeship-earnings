using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship.Events;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.ApprenticeshipFunding;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship
{
    public class Apprenticeship : AggregateRoot
    {
        public Apprenticeship(Guid apprenticeshipKey, long approvalsApprenticeshipId, string uln, long ukprn, long employerAccountId, string legalEntityName, DateTime actualStartDate, DateTime plannedEndDate, decimal agreedPrice, string trainingCode, long? fundingEmployerAccountId, FundingType fundingType, decimal fundingBandMaximum, int ageAtStartOfApprenticeship)
        {
            ApprenticeshipKey = apprenticeshipKey;
            ApprovalsApprenticeshipId = approvalsApprenticeshipId;
            Uln = uln;
            UKPRN = ukprn;
            EmployerAccountId = employerAccountId;
            LegalEntityName = legalEntityName;
            ActualStartDate = actualStartDate;
            PlannedEndDate = plannedEndDate;
            AgreedPrice = agreedPrice;
            TrainingCode = trainingCode;
            FundingEmployerAccountId = fundingEmployerAccountId;
            FundingType = fundingType;
            FundingBandMaximum = fundingBandMaximum;
            AgeAtStartOfApprenticeship = ageAtStartOfApprenticeship;
        }

        public Guid ApprenticeshipKey { get; }
        public long ApprovalsApprenticeshipId { get; }
        public string Uln { get; }
        public long UKPRN { get; }
        public long EmployerAccountId { get; }
        public string LegalEntityName { get; }
        public DateTime ActualStartDate { get; }
        public DateTime PlannedEndDate { get; }
        public decimal AgreedPrice { get; }
        public string TrainingCode { get; }
        public long? FundingEmployerAccountId { get; }
        public FundingType FundingType { get; }
        public decimal FundingBandMaximum { get; }
        public int AgeAtStartOfApprenticeship { get; }
        public EarningsProfile? EarningsProfile { get; private set; }

        public string FundingLineType =>
            AgeAtStartOfApprenticeship < 19
                ? "16-18 Apprenticeship (Employer on App Service)"
                : "19+ Apprenticeship (Employer on App Service)";

        public void CalculateEarnings()
        {
            var apprenticeshipFunding = new ApprenticeshipFunding.ApprenticeshipFunding(AgreedPrice, ActualStartDate, PlannedEndDate, FundingBandMaximum);
            var earnings = apprenticeshipFunding.GenerateEarnings();
            EarningsProfile = new EarningsProfile(apprenticeshipFunding.OnProgramTotal, earnings.Select(x => new Instalment(x.AcademicYear, x.DeliveryPeriod, x.Amount)).ToList(), apprenticeshipFunding.CompletionPayment, Guid.NewGuid());
            AddEvent(new EarningsCalculatedEvent(this));
        }

        public void RecalculateEarnings(EarningsProfile existingEarningsProfile, DateTime effectiveFromDate)
        {
            var apprenticeshipFunding = new ApprenticeshipFunding.ApprenticeshipFunding(AgreedPrice, ActualStartDate, PlannedEndDate, FundingBandMaximum);
            var existingEarnings = existingEarningsProfile.Instalments.Select(x => new Earning { AcademicYear = x.AcademicYear, Amount = x.Amount, DeliveryPeriod = x.DeliveryPeriod }).ToList();
            var earnings = apprenticeshipFunding.RecalculateEarnings(existingEarnings, effectiveFromDate);
            EarningsProfile = new EarningsProfile(apprenticeshipFunding.OnProgramTotal, earnings.Select(x => new Instalment(x.AcademicYear, x.DeliveryPeriod, x.Amount)).ToList(), apprenticeshipFunding.CompletionPayment, Guid.NewGuid());
            AddEvent(new EarningsRecalculatedEvent(this));
        }
    }
}
