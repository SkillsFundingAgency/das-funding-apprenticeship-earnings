using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using System.Collections.ObjectModel;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models;

public abstract class BaseLearning : AggregateRoot
{
    public abstract Guid LearningKey { get; }
    public abstract long ApprovalsApprenticeshipId { get; }
    public abstract string Uln { get; }
    public abstract bool HasEHCP { get; }
    public abstract bool IsCareLeaver { get; }
    public abstract bool CareLeaverEmployerConsentGiven { get; }
    public abstract DateTime DateOfBirth { get; }

    public abstract void UpdateDateOfBirth(DateTime dateOfBirth);
    public abstract BaseEpisode GetEpisode(Guid episodeKey);
    public abstract BaseEpisode GetFirstEpisode();
}

public abstract class BaseLearning<TEntity, TEpisode> : BaseLearning
    where TEntity : BaseLearningEntity
    where TEpisode : BaseEpisode
{
    protected TEntity _entity;
    protected List<TEpisode> _episodes;

    public override Guid LearningKey => _entity.LearningKey;
    public override long ApprovalsApprenticeshipId => _entity.ApprovalsApprenticeshipId;
    public override string Uln => _entity.Uln;
    public override bool HasEHCP => _entity?.HasEHCP ?? false;
    public override bool IsCareLeaver => _entity?.IsCareLeaver ?? false;
    public override bool CareLeaverEmployerConsentGiven => _entity?.CareLeaverEmployerConsentGiven ?? false;
    public override DateTime DateOfBirth => _entity.DateOfBirth;
    public IReadOnlyCollection<TEpisode> Episodes => new ReadOnlyCollection<TEpisode>(_episodes);

    public override BaseEpisode GetEpisode(Guid episodeKey)
    {
        var episode = _episodes.SingleOrDefault(e => e.EpisodeKey == episodeKey);
        if (episode == null)
            throw new InvalidOperationException($"No episode found for key {episodeKey}");
        return episode;
    }

    public override BaseEpisode GetFirstEpisode()
    {
        var episode = _episodes.FirstOrDefault();
        if (episode == null)
            throw new InvalidOperationException("No episodes found for learning");
        return episode;
    }

#pragma warning disable CS8618
    protected BaseLearning(TEntity entity)
    {
        _entity = entity;
    }
#pragma warning restore CS8618

    public BaseLearningEntity GetModel()
    {
        return _entity;
    }
}

#pragma warning disable CS8618
public class ShortCourseUpdateModel
{
    public string Uln { get; set; }
    public string CourseCode { get; set; }
    public long EmployerId { get; set; }
    public long Ukprn { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? WithdrawalDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    public DateTime ExpectedEndDate { get; set; }
    public decimal TotalPrice { get; set; }
}
#pragma warning restore CS8618
