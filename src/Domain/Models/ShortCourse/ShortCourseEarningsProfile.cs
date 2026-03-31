using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using System.Collections.ObjectModel;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.ShortCourse;

public class ShortCourseEarningsProfile : BaseEarningsProfile<ShortCourseEarningsProfileEntity>
{
    private List<ShortCourseInstalment> _instalments;

    public IReadOnlyCollection<ShortCourseInstalment> Instalments => new ReadOnlyCollection<ShortCourseInstalment>(_instalments);

    public ShortCourseEarningsProfile(ShortCourseEarningsProfileEntity model, Action<AggregateComponent> addChildToRoot) : base(model, addChildToRoot)
    {
        _instalments = model.Instalments?.Select(ShortCourseInstalment.Get).ToList() ?? new List<ShortCourseInstalment>();
    }

    public static ShortCourseEarningsProfile Get(ShortCourseEpisode episode, ShortCourseEarningsProfileEntity model)
    {
        return new ShortCourseEarningsProfile(model, episode.AddChildToRoot);
    }

    public ShortCourseEarningsProfile(decimal onProgramTotal,
        List<ShortCourseInstalment> instalments,
        decimal completionPayment,
        Guid episodeKey,
        bool isApproved,
        Action<AggregateComponent> addChildToRoot,
        string calculationData) : base(onProgramTotal, completionPayment, episodeKey, isApproved, calculationData, addChildToRoot)
    {
        Entity.Instalments = instalments.ToModels<ShortCourseInstalment, ShortCourseInstalmentEntity>(model => model.EarningsProfileId = EarningsProfileId);
        _instalments = instalments;

        AddEvent(Entity.CreatedEarningsProfileUpdatedEvent(true));
    }

    public void Update(
        decimal? onProgramTotal = null,
        List<ShortCourseInstalment>? instalments = null,
        decimal? completionPayment = null,
        string? calculationData = null
    )
    {
        var versionChanged = false;

        if (onProgramTotal.HasValue && Entity.OnProgramTotal != onProgramTotal.Value)
        {
            Entity.OnProgramTotal = onProgramTotal.Value;
            versionChanged = true;
        }

        if (instalments != null && !instalments.AreSame(Entity.Instalments))
        {
            Entity.Instalments = instalments!.ToModels<ShortCourseInstalment, ShortCourseInstalmentEntity>();
            _instalments = instalments!;
            versionChanged = true;
        }


        if (completionPayment.HasValue && Entity.CompletionPayment != completionPayment.Value)
        {
            Entity.CompletionPayment = completionPayment.Value;
            versionChanged = true;
        }

        if (!string.IsNullOrEmpty(calculationData) && Entity.CalculationData != calculationData)
        {
            Entity.CalculationData = calculationData;
            versionChanged = true;
        }

        if (versionChanged)
        {
            Entity.Version = Guid.NewGuid();
            PurgeEventsOfType<ShortCourseEarningsProfileUpdatedEvent>();
            var archiveEvent = Entity.CreatedEarningsProfileUpdatedEvent();
            AddEvent(archiveEvent);
        }
    }
}
