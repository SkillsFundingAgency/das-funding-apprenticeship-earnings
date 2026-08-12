using SFA.DAS.Learning.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataTransferObjects
{
    public class Learner
    {
        public Learner(string uln, List<OnProgrammeEarning> onProgrammeEarnings, decimal totalOnProgrammeEarnings, bool isNoneLevyFullyFunded)
        {
            Uln = uln;
            OnProgrammeEarnings = onProgrammeEarnings;
            TotalOnProgrammeEarnings = totalOnProgrammeEarnings;
            IsNoneLevyFullyFunded = isNoneLevyFullyFunded;
        }

        public string Uln { get; set; }
        public List<OnProgrammeEarning> OnProgrammeEarnings { get; set; }
        public decimal TotalOnProgrammeEarnings { get; set; }
        public bool IsNoneLevyFullyFunded { get; set; }
    }
}