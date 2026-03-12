using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.ShortCourse;

public class ShortCourseLearning : BaseLearning<ShortCourseLearningEntity, ShortCourseEpisode>
{

    private ShortCourseLearning(ShortCourseLearningEntity entity) : base(entity)
    {
        _episodes = _entity.Episodes.Select(this.GetShortCourseEpisodeFromEntity).ToList();
    }

    public static ShortCourseLearning Get(ShortCourseLearningEntity entity)
    {
        return new ShortCourseLearning(entity);
    }

    public new ShortCourseLearningEntity GetModel()
    {
        return _entity;
    }

    public ShortCourseEpisode GetEpisode()
    {
        // Intentionally fail if there are more as at the time of writing there should only ever be one episode for short course learning
        // if this changes in the future this method will need to be rethought
        return _episodes.Single();
    }

    public void UpdateUnapprovedShortCourseInformation(ShortCourseUpdateModel updateModel)
    {
        _entity.Uln = updateModel.Uln;
        var episode = _entity.Episodes.Single();
        episode.TrainingCode = updateModel.CourseCode;
        episode.EmployerAccountId = updateModel.EmployerId;
        episode.Ukprn = updateModel.Ukprn;
        episode.StartDate = updateModel.StartDate;
        episode.WithdrawalDate = updateModel.WithdrawalDate;
        episode.CompletionDate = updateModel.CompletionDate;
        episode.EndDate = updateModel.ExpectedEndDate;
        episode.CoursePrice = updateModel.TotalPrice;
    }

    public override void UpdateDateOfBirth(DateTime dateOfBirth)
    {
        _entity.DateOfBirth = dateOfBirth;
    }
}
