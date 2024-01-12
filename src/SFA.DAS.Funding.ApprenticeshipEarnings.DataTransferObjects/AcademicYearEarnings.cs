namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataTransferObjects
{
    public class AcademicYearEarnings
    {
        public AcademicYearEarnings(List<Learner> learners)
        {
            Learners = learners;
        }

        public List<Learner> Learners { get; set; }
    }
}