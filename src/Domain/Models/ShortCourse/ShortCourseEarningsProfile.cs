using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using System.Collections.ObjectModel;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.ShortCourse;

public class ShortCourseEarningsProfile : BaseEarningsProfile<ShortCourseEarningsProfileEntity>
{
    private List<ShortCourseInstalment> _instalments;

    public IReadOnlyCollection<ShortCourseInstalment> Instalments => new ReadOnlyCollection<ShortCourseInstalment>(_instalments);


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

        //AddEvent(Entity.CreatedEarningsProfileUpdatedEvent(true)); TODO Add SC event for this
    }

    public void Update(
        ISystemClockService systemClock,
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
            //TODO Add SC event for this


            //Entity.Version = Guid.NewGuid();
            //PurgeEventsOfType<EarningsProfileUpdatedEvent>();// Remove previous update events so only the latest is kept
            //var archiveEvent = Entity.CreatedEarningsProfileUpdatedEvent();
            //AddEvent(archiveEvent);
        }
    }
}
