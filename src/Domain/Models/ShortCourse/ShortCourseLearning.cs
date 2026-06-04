using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using SFA.DAS.Learning.Types;
using FundingType = SFA.DAS.Learning.Types.FundingType;

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

    public override void Approve(Guid episodeKey) => GetShortCourseEpisode(episodeKey).Approve();

    public void Remove(Guid episodeKey) => GetShortCourseEpisode(episodeKey).Remove();

    public void UpdateOnProgramme(Guid episodeKey, DateTime? completionDate, DateTime? withdrawalDate, List<Milestone> milestones, string calculationData)
    {
        var episode = GetShortCourseEpisode(episodeKey);
        episode.UpdateCompletion(completionDate);
        episode.UpdateWithdrawalDate(withdrawalDate);
        episode.UpdateMilestones(milestones);
        episode.CalculateShortCourseOnProgram(calculationData);
    }

    public void CalculateOnProgram(Guid episodeKey, string calculationData)
        => GetShortCourseEpisode(episodeKey).CalculateShortCourseOnProgram(calculationData);

    public void UpdateUnapprovedShortCourseInformation(Guid episodeKey, ShortCourseUpdateModel updateModel)
    {
        _entity.Uln = updateModel.Uln;
        var episode = _entity.Episodes.Single(e => e.Key == episodeKey);
        episode.TrainingCode = updateModel.CourseCode;
        episode.Ukprn = updateModel.Ukprn;
        episode.StartDate = updateModel.StartDate;
        episode.WithdrawalDate = updateModel.WithdrawalDate;
        episode.CompletionDate = updateModel.CompletionDate;
        episode.EndDate = updateModel.ExpectedEndDate;
        episode.CoursePrice = updateModel.TotalPrice;
        episode.Milestones = updateModel.Milestones.ToMilestoneFlags();
    }

    public bool HasEpisode(Guid episodeKey)
        => _entity.Episodes.Any(e => e.Key == episodeKey);

    public void AddUnapprovedEpisode(CreateUnapprovedShortCourseLearningRequest request)
    {
        var episodeEntity = new ShortCourseEpisodeEntity
        {
            Key = request.EpisodeKey,
            LearningKey = request.LearningKey,
            Ukprn = request.OnProgramme.Ukprn,
            FundingType = FundingType.Levy,
            TrainingCode = request.OnProgramme.CourseCode,
            CompletionDate = request.OnProgramme.CompletionDate,
            WithdrawalDate = request.OnProgramme.WithdrawalDate,
            StartDate = request.OnProgramme.StartDate,
            EndDate = request.OnProgramme.ExpectedEndDate,
            CoursePrice = request.OnProgramme.TotalPrice,
            Milestones = request.OnProgramme.Milestones.ToMilestoneFlags()
        };
        _entity.Episodes.Add(episodeEntity);
        _episodes.Add(this.GetShortCourseEpisodeFromEntity(episodeEntity));
    }

    public override void UpdateDateOfBirth(DateTime dateOfBirth)
    {
        _entity.DateOfBirth = dateOfBirth;
    }

    private ShortCourseEpisode GetShortCourseEpisode(Guid episodeKey)
    {
        return _episodes.SingleOrDefault(e => e.EpisodeKey == episodeKey)
            ?? throw new InvalidOperationException($"No episode found for key {episodeKey}");
    }
}
