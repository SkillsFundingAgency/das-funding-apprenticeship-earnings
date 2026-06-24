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
    public MilestoneFlags MilestoneFlags => _entity.Milestones;
    public bool IsApproved => _earningsProfile?.IsApproved ?? false;
    public bool IsRemoved => _entity.IsRemoved;

    private ShortCourseEpisode(ShortCourseEpisodeEntity model, DateTime dateOfBirth, Action<AggregateComponent> addChildToRoot) : base(model, addChildToRoot)
    {
        if (_entity.EarningsProfile != null)
            _earningsProfile = new ShortCourseEarningsProfile(_entity.EarningsProfile, addChildToRoot);

        UpdateAgeAtStart(dateOfBirth);
    }

    internal static ShortCourseEpisode Get(ShortCourseLearning learning, ShortCourseEpisodeEntity entity)
    {
        var episode = new ShortCourseEpisode(entity, learning.DateOfBirth, learning.AddChildToRoot);
        return episode;
    }

    public void CalculateShortCourseOnProgram(string calculationData, bool suppressThirtyPercent = false)
    {
        _entity.IsRemoved = false;

        var onProgramPayments = ShortCoursePayments.GenerateShortCoursePayments(
            CoursePrice,
            StartDate,
            EndDate,
            CompletionDate,
            suppressThirtyPercent);

        if(WithdrawalDate.HasValue)
            ShortCoursePayments.RemoveWithdrawnPayments(onProgramPayments, _entity.Milestones);

        ShortCoursePayments.SetPayability(onProgramPayments, _earningsProfile?.IsApproved ?? false, _entity.Milestones);

        var onProgramTotal = suppressThirtyPercent ? 0m : ShortCoursePayments.CalculateThirtyPercentInstalmentAmount(CoursePrice);

        if (_earningsProfile == null)
        {
            _earningsProfile = new ShortCourseEarningsProfile(
                onProgramTotal,
                onProgramPayments,
                ShortCoursePayments.CalculateCompletionInstalmentAmount(CoursePrice),
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
                onProgramTotal: onProgramTotal,
                completionPayment: ShortCoursePayments.CalculateCompletionInstalmentAmount(CoursePrice));

            _entity.EarningsProfile = _earningsProfile.GetModel();
        }
    }

    private void RemoveEarnings()
    {
        if (_earningsProfile == null) return;

        _earningsProfile.Update(
            instalments: [],
            onProgramTotal: 0m,
            completionPayment: 0m,
            calculationData: "{}");
        _entity.EarningsProfile = _earningsProfile.GetModel();
    }

    public void UpdateAgeAtStart(DateTime dateOfBirth)
    {
        _ageAtStartOfApprenticeship = dateOfBirth.CalculateAgeAtDate(StartDate);
    }

    public override void Approve(long employerAccountId, long fundingAccountId, Guid learnerKey, string learnerRef)
    {
        _earningsProfile!.Approve();
        ShortCoursePayments.SetPayability(_earningsProfile.Instalments.ToList(), true, _entity.Milestones);
        AddEvent(new ShortCoursePayableEarningsUpdatedEvent
        {
            LearningKey = _entity.LearningKey,
            EpisodeKey = EpisodeKey,
            EmployerAccountId = employerAccountId,
            FundingAccountId = fundingAccountId,
            LearnerKey = learnerKey,
            LearnerRef = learnerRef
        });
    }

    public void Remove()
    {
        _entity.IsRemoved = true;
        RemoveEarnings();
    }

    public void UpdateWithdrawalDate(DateTime? withdrawalDate)
    {
        _entity.WithdrawalDate = withdrawalDate;
    }

    public void UpdateStartDate(DateTime startDate)
    {
        _entity.StartDate = startDate;
    }

    public void UpdateExpectedEndDate(DateTime expectedEndDate)
    {
        _entity.EndDate = expectedEndDate;
    }

    public void UpdateMilestones(List<Milestone> milestones)
    {
        _entity.Milestones = milestones.ToMilestoneFlags();
    }
}