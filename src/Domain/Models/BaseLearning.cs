using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using System.Collections.ObjectModel;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models;

public abstract class BaseLearning<TEntity, TEpisode> : AggregateRoot 
    where TEntity : BaseLearningEntity
{
    protected TEntity _entity;
    protected List<TEpisode> _episodes;

    public Guid LearningKey => _entity.LearningKey;
    public long ApprovalsApprenticeshipId => _entity.ApprovalsApprenticeshipId;
    public string Uln => _entity.Uln;
    public bool HasEHCP => _entity?.HasEHCP ?? false;
    public bool IsCareLeaver => _entity?.IsCareLeaver ?? false;
    public bool CareLeaverEmployerConsentGiven => _entity?.CareLeaverEmployerConsentGiven ?? false;
    public DateTime DateOfBirth => _entity.DateOfBirth;
    public IReadOnlyCollection<TEpisode> Episodes => new ReadOnlyCollection<TEpisode>(_episodes);

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