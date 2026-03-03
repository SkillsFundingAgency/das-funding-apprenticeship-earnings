using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.ShortCourse;

public class ShortCourseEpisode : BaseEpisode<ShortCourseEpisodeEntity, ShortCourseEarningsProfile>
{

    private int _ageAtStartOfApprenticeship;

    public DateTime StartDate => _model.StartDate;
    public DateTime EndDate => _model.EndDate;
    public decimal CoursePrice => _model.CoursePrice;

    private ShortCourseEpisode(ShortCourseEpisodeEntity model, DateTime dateOfBirth, Action<AggregateComponent> addChildToRoot) : base(model, dateOfBirth, addChildToRoot)
    {
        UpdateAgeAtStart(model.StartDate);
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

            _model.EarningsProfile = _earningsProfile.GetModel();
        }
        else
        {
            _earningsProfile.Update(systemClock,
                instalments: onProgramPayments,
                calculationData: calculationData,
                onProgramTotal: onProgramPayments.Where(x => x.Type == InstalmentType.Regular || x.Type == InstalmentType.Balancing).Sum(x => x.Amount),
                completionPayment: onProgramPayments.Where(x => x.Type == InstalmentType.Completion).Sum(x => x.Amount));

            _model.EarningsProfile = _earningsProfile.GetModel();
        }
    }

}