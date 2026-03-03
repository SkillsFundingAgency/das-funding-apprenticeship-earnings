using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.ShortCourse;

public class ShortCourseEpisode : BaseEpisode<ShortCourseEpisodeEntity, ShortCourseEarningsProfile>
{
    public DateTime StartDate => _entity.StartDate;
    public DateTime EndDate => _entity.EndDate;
    public decimal CoursePrice => _entity.CoursePrice;

    private ShortCourseEpisode(ShortCourseEpisodeEntity model, DateTime dateOfBirth, Action<AggregateComponent> addChildToRoot) : base(model, addChildToRoot)
    {
        UpdateAgeAtStart(dateOfBirth);
    }

    internal static ShortCourseEpisode Get(Learning learning, ShortCourseEpisodeEntity entity)
    {
        var episode = new ShortCourseEpisode(entity, learning.DateOfBirth, learning.AddChildToRoot);
        return episode;
    }

    public void CalculateShortCourseOnProgram(Learning learning, ISystemClockService systemClock, bool isApproved, string calculationData)
    {
        var currentEpisode = learning.ShortCourseEpisodes.Single();

        var onProgramPayments = ShortCoursePayments.GenerateShortCoursePayments(
            CoursePrice,
            StartDate,
            EndDate,
            currentEpisode.CompletionDate);

        if (_earningsProfile == null)
        {
            _earningsProfile = new ShortCourseEarningsProfile(
                onProgramPayments.Where(x => x.Type == InstalmentType.Regular || x.Type == InstalmentType.Balancing).Sum(x => x.Amount),
                onProgramPayments,
                onProgramPayments.Where(x => x.Type == InstalmentType.Completion).Sum(x => x.Amount),
                EpisodeKey, 
                isApproved, 
                this.AddChildToRoot, 
                calculationData);

            _entity.EarningsProfile = _earningsProfile.GetModel();
        }
        else
        {
            _earningsProfile.Update(systemClock,
                instalments: onProgramPayments,
                calculationData: calculationData,
                onProgramTotal: onProgramPayments.Where(x => x.Type == InstalmentType.Regular || x.Type == InstalmentType.Balancing).Sum(x => x.Amount),
                completionPayment: onProgramPayments.Where(x => x.Type == InstalmentType.Completion).Sum(x => x.Amount));

            _entity.EarningsProfile = _earningsProfile.GetModel();
        }
    }

    public void UpdateAgeAtStart(DateTime dateOfBirth)
    {
        _ageAtStartOfApprenticeship = dateOfBirth.CalculateAgeAtDate(StartDate);
    }
}