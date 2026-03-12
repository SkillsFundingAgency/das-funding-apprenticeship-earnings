using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.ShortCourse;

public class ShortCourseEpisode : BaseEpisode<ShortCourseEpisodeEntity, ShortCourseEarningsProfile>
{
    public DateTime StartDate => _entity.StartDate;
    public DateTime EndDate => _entity.EndDate;
    public decimal CoursePrice => _entity.CoursePrice;

    private ShortCourseEpisode(ShortCourseEpisodeEntity model, DateTime dateOfBirth, Action<AggregateComponent> addChildToRoot) : base(model, addChildToRoot)
    {
        if (_entity.EarningsProfile != null)
        {
            _earningsProfile = ShortCourseEarningsProfile.Get(this, _entity.EarningsProfile);
        }

        UpdateAgeAtStart(dateOfBirth);
    }

    internal static ShortCourseEpisode Get(ShortCourseLearning learning, ShortCourseEpisodeEntity entity)
    {
        var episode = new ShortCourseEpisode(entity, learning.DateOfBirth, learning.AddChildToRoot);
        return episode;
    }

    public void CalculateShortCourseOnProgram(string calculationData)
    {
        var onProgramPayments = ShortCoursePayments.GenerateShortCoursePayments(
            CoursePrice,
            StartDate,
            EndDate,
            CompletionDate);

        if(WithdrawalDate.HasValue)
            ShortCoursePayments.RemoveWithdrawnPayments(onProgramPayments, _entity.Milestones);

        if (_earningsProfile == null)
        {
            _earningsProfile = new ShortCourseEarningsProfile(
                onProgramPayments.Where(x => x.Type == ShortCourseInstalmentType.ThirtyPercentLearningComplete).Sum(x => x.Amount),
                onProgramPayments,
                onProgramPayments.Where(x => x.Type == ShortCourseInstalmentType.LearningComplete).Sum(x => x.Amount),
                EpisodeKey, 
                false, // Initialize as unapproved
                this.AddChildToRoot, 
                calculationData);

            _entity.EarningsProfile = _earningsProfile.GetModel();
        }
        else
        {
            _earningsProfile.Update(
                instalments: onProgramPayments,
                calculationData: calculationData,
                onProgramTotal: onProgramPayments.Where(x => x.Type == ShortCourseInstalmentType.ThirtyPercentLearningComplete).Sum(x => x.Amount),
                completionPayment: onProgramPayments.Where(x => x.Type == ShortCourseInstalmentType.LearningComplete).Sum(x => x.Amount));

            _entity.EarningsProfile = _earningsProfile.GetModel();
        }
    }

    public void UpdateAgeAtStart(DateTime dateOfBirth)
    {
        _ageAtStartOfApprenticeship = dateOfBirth.CalculateAgeAtDate(StartDate);
    }

    public override void Approve() => _earningsProfile!.Approve();

    public void UpdateWithdrawalDate(DateTime? withdrawalDate)
    {
        _entity.WithdrawalDate = withdrawalDate;
    }

    public void UpdateMilestones(List<Milestone> milestones)
    {
        _entity.Milestones = milestones.ToMilestoneFlags();
    }
}