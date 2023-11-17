using SFA.DAS.Apprenticeships.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataTransferObjects
{
    public class Learner
    {
        public Learner(string uln, FundingType fundingType, List<OnProgrammeEarning> onProgrammeEarnings, decimal totalOnProgrammeEarnings)
        {
            Uln = uln;
            FundingType = fundingType;
            OnProgrammeEarnings = onProgrammeEarnings;
            TotalOnProgrammeEarnings = totalOnProgrammeEarnings;
        }

        public string Uln { get; set; }
        public FundingType FundingType { get; set; }
        public List<OnProgrammeEarning> OnProgrammeEarnings { get; set; }
        public decimal TotalOnProgrammeEarnings { get; set; }
    }
}