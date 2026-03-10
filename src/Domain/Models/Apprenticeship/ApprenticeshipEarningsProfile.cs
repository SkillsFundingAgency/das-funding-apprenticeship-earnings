using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.EnglishAndMaths;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using System.Collections.ObjectModel;
using EnglishAndMathsDomainModel = SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.EnglishAndMaths.EnglishAndMaths;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;

public class ApprenticeshipEarningsProfile : BaseEarningsProfile<ApprenticeshipEarningsProfileEntity>
{
    private List<ApprenticeshipInstalment> _instalments;

    public IReadOnlyCollection<ApprenticeshipInstalment> Instalments => new ReadOnlyCollection<ApprenticeshipInstalment>(_instalments);
    public IReadOnlyCollection<AdditionalPayment> AdditionalPayments => Entity.ApprenticeshipAdditionalPayments.Select(AdditionalPayment.Get).ToList().AsReadOnly();
    public IReadOnlyCollection<EnglishAndMathsDomainModel> MathsAndEnglishCourses => Entity.EnglishAndMathsCourses.Select(EnglishAndMathsDomainModel.Get).ToList().AsReadOnly();

    public ApprenticeshipEarningsProfile(decimal onProgramTotal,
        List<ApprenticeshipInstalment> instalments,
        List<AdditionalPayment> additionalPayments,
        List<EnglishAndMathsDomainModel> mathsAndEnglishCourses,
        decimal completionPayment,
        Guid episodeKey,
        bool isApproved,
        Action<AggregateComponent> addChildToRoot,
        string calculationData): base(onProgramTotal, completionPayment, episodeKey, isApproved, calculationData, addChildToRoot)
    {
        Entity.Instalments = instalments.ToModels<ApprenticeshipInstalment, ApprenticeshipInstalmentEntity>(model=> model.EarningsProfileId = EarningsProfileId);
        Entity.ApprenticeshipAdditionalPayments = additionalPayments.ToModels<AdditionalPayment, ApprenticeshipAdditionalPaymentEntity>(model => model.EarningsProfileId = EarningsProfileId);
        Entity.EnglishAndMathsCourses = mathsAndEnglishCourses.ToModels((EnglishAndMathsEntity model) => model.EarningsProfileId = EarningsProfileId);
        _instalments = instalments;

        AddEvent(Entity.CreatedEarningsProfileUpdatedEvent(true));
    }

    public ApprenticeshipEarningsProfile(ApprenticeshipEarningsProfileEntity model, Action<AggregateComponent> addChildToRoot) : base(model, addChildToRoot)
    {
        _instalments = model.Instalments?.Select(ApprenticeshipInstalment.Get).ToList() ?? new List<ApprenticeshipInstalment>();
    }
    public void Update(
        ISystemClockService systemClock,
        decimal? onProgramTotal = null, 
        List<ApprenticeshipInstalment>? instalments = null, 
        List<AdditionalPayment>? additionalPayments = null, 
        List<EnglishAndMathsDomainModel>? mathsAndEnglishCourses = null,
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
            Entity.Instalments = instalments!.ToModels<ApprenticeshipInstalment, ApprenticeshipInstalmentEntity>();
            _instalments = instalments!;
            versionChanged = true;
        }
            
        if (additionalPayments != null && !additionalPayments.AreSame(Entity.ApprenticeshipAdditionalPayments))
        {
            Entity.ApprenticeshipAdditionalPayments = additionalPayments!.ToModels<AdditionalPayment, ApprenticeshipAdditionalPaymentEntity>();
            versionChanged = true;
        }

        if (mathsAndEnglishCourses != null && !mathsAndEnglishCourses.AreSame(Entity.EnglishAndMathsCourses))
        {
            UpdateEnglishAndMathsCourses(mathsAndEnglishCourses);
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
            PurgeEventsOfType<EarningsProfileUpdatedEvent>();// Remove previous update events so only the latest is kept
            var archiveEvent = Entity.CreatedEarningsProfileUpdatedEvent();
            AddEvent(archiveEvent);
        }
    }

    /// <summary>
    /// Some payments are not calculated, but are instead added to the earnings profile via an external process.
    /// In the event of a recalculation, these payments should be preserved.
    /// This method returns the list of payments that are not calculated.
    /// </summary>
    public IReadOnlyCollection<AdditionalPayment> PersistentAdditionalPayments()
    {
        return Entity.ApprenticeshipAdditionalPayments
            .Where(x=> x.AdditionalPaymentType == InstalmentTypes.LearningSupport)
            .Select(AdditionalPayment.Get)
            .ToList().AsReadOnly();
    }

    /// <summary>
    /// Maths and English courses are not calculated, but are instead added to the earnings profile via an external process.
    /// In the event of a recalculation, these should be preserved.
    /// This method returns the courses so that they can be preserved.
    /// </summary>
    public IReadOnlyCollection<EnglishAndMathsDomainModel> PersistentMathsAndEnglishCourses()
    {
        if (Entity.EnglishAndMathsCourses == null)
            return new List<EnglishAndMathsDomainModel>();

        return Entity.EnglishAndMathsCourses
            .Select(EnglishAndMathsDomainModel.Get)
            .ToList().AsReadOnly();
    }

    public static ApprenticeshipEarningsProfile Get(ApprenticeshipEpisode episode, ApprenticeshipEarningsProfileEntity model)
    {
        var profile =  new ApprenticeshipEarningsProfile(model, episode.AddChildToRoot);
        return profile;
    }

    private void UpdateEnglishAndMathsCourses(List<EnglishAndMathsDomainModel> updatedCourses)
    {
        Entity.EnglishAndMathsCourses ??= new List<EnglishAndMathsEntity>();

        // 1. If nothing remains, clear everything
        if (updatedCourses.Count == 0)
        {
            Entity.EnglishAndMathsCourses.Clear();
            return;
        }

        // 2. Sync courses by (LearnAimRef, StartDate)
        Entity.EnglishAndMathsCourses.SyncByKey(
            updatedCourses,
            existingKey: c => (c.LearnAimRef, c.StartDate),
            updatedKey: c => (c.LearnAimRef, c.StartDate),
            updateExisting: (existing, updated) =>
            {
                existing.StartDate = updated.StartDate;
                existing.EndDate = updated.EndDate;
                existing.Amount = updated.Amount;
                existing.WithdrawalDate = updated.WithdrawalDate;
                existing.CompletionDate = updated.CompletionDate;
                existing.PauseDate = updated.PauseDate;
                existing.PriorLearningAdjustmentPercentage = updated.PriorLearningAdjustmentPercentage;

                // 3a. Sync Instalments
                existing.Instalments.SyncByKey(
                    updated.Instalments,
                    existingKey: i => (i.AcademicYear, i.DeliveryPeriod, i.Type),
                    updatedKey: i => (i.AcademicYear, i.DeliveryPeriod, i.Type.ToString()),
                    updateExisting: (ex, up) =>
                    {
                        ex.Amount = up.Amount;
                    },
                    createNew: up => up.GetEntity()
                );

                // 3b. Sync Periods in Learning
                existing.PeriodsInLearning.SyncByKey(
                    updated.PeriodsInLearning,
                    existingKey: p => p.StartDate.Date,
                    updatedKey: p => p.StartDate.Date,
                    updateExisting: (ex, up) =>
                    {
                        ex.EndDate = up.EndDate;
                        ex.OriginalExpectedEndDate = up.OriginalExpectedEndDate;
                    },
                    createNew: up => up.GetEntity()
                );
            },
            createNew: updated => updated.GetEntity()
        );
    }
}