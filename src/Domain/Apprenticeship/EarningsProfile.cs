using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using System.Collections.ObjectModel;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

public class EarningsProfile : AggregateComponent
{
    private EarningsProfileModel Model { get; set; }

    private List<Instalment> _instalments;

    public EarningsProfile(decimal onProgramTotal,
        List<Instalment> instalments,
        List<AdditionalPayment> additionalPayments,
        List<MathsAndEnglish> mathsAndEnglishCourses,
        decimal completionPayment,
        Guid episodeKey,
        bool isApproved,
        Action<AggregateComponent> addChildToRoot,
        string calculationData): base(addChildToRoot)
    {
        var earningProfileId = Guid.NewGuid();

        Model = new EarningsProfileModel();
        Model.EarningsProfileId = earningProfileId;
        Model.OnProgramTotal = onProgramTotal;
        Model.Instalments = instalments.ToModels<Instalment, InstalmentModel>(model=> model.EarningsProfileId = earningProfileId);
        Model.AdditionalPayments = additionalPayments.ToModels<AdditionalPayment, AdditionalPaymentModel>(model => model.EarningsProfileId = earningProfileId);
        Model.MathsAndEnglishCourses = mathsAndEnglishCourses.ToModels<MathsAndEnglish, MathsAndEnglishModel>(model => model.EarningsProfileId = earningProfileId);
        _instalments = instalments;
        Model.CompletionPayment = completionPayment;
        Model.EpisodeKey = episodeKey;
        Model.Version = Guid.NewGuid();
        Model.IsApproved = isApproved;
        Model.CalculationData = calculationData;

        AddEvent(Model.CreatedEarningsProfileUpdatedEvent(true));
    }

    public EarningsProfile(EarningsProfileModel model, Action<AggregateComponent> addChildToRoot) : base(addChildToRoot)
    {
        Model = model;
        _instalments = model.Instalments?.Select(Instalment.Get).ToList() ?? new List<Instalment>();
    }

    public Guid EarningsProfileId => Model.EarningsProfileId;
    public decimal OnProgramTotal => Model.OnProgramTotal;
    public IReadOnlyCollection<Instalment> Instalments => new ReadOnlyCollection<Instalment>(_instalments);
    public IReadOnlyCollection<AdditionalPayment> AdditionalPayments => Model.AdditionalPayments.Select((AdditionalPayment.Get)).ToList().AsReadOnly();
    public IReadOnlyCollection<MathsAndEnglish> MathsAndEnglishCourses => Model.MathsAndEnglishCourses.Select((MathsAndEnglish.Get)).ToList().AsReadOnly();
    public decimal CompletionPayment => Model.CompletionPayment;
    public Guid Version => Model.Version;
    public bool IsApproved => Model.IsApproved;
    public string CalculationData => Model.CalculationData;

    public void Update(
        ISystemClockService systemClock,
        decimal? onProgramTotal = null, 
        List<Instalment>? instalments = null, 
        List<AdditionalPayment>? additionalPayments = null, 
        List<MathsAndEnglish>? mathsAndEnglishCourses = null,
        decimal? completionPayment = null,
        string? calculationData = null
    )
    {
        var versionChanged = false;

        if (onProgramTotal.HasValue && Model.OnProgramTotal != onProgramTotal.Value)
        {
            Model.OnProgramTotal = onProgramTotal.Value;
            versionChanged = true;
        }

        if (instalments != null && !instalments.AreSame(Model.Instalments))
        {
            Model.Instalments = instalments!.ToModels<Instalment,InstalmentModel>();
            _instalments = instalments!;
            versionChanged = true;
        }
            
        if (additionalPayments != null && !additionalPayments.AreSame(Model.AdditionalPayments))
        {
            Model.AdditionalPayments = additionalPayments!.ToModels<AdditionalPayment, AdditionalPaymentModel>();
            versionChanged = true;
        }

        if (mathsAndEnglishCourses != null && !mathsAndEnglishCourses.AreSame(Model.MathsAndEnglishCourses))
        {
            UpdateEnglishAndMathsCourses(mathsAndEnglishCourses);
            versionChanged = true;
        }

        if (completionPayment.HasValue && Model.CompletionPayment != completionPayment.Value)
        {
            Model.CompletionPayment = completionPayment.Value;
            versionChanged = true;
        }

        if (!string.IsNullOrEmpty(calculationData) && Model.CalculationData != calculationData)
        {
            Model.CalculationData = calculationData;
            versionChanged = true;
        }

        if (versionChanged)
        {
            Model.Version = Guid.NewGuid();
            PurgeEventsOfType<EarningsProfileUpdatedEvent>();// Remove previous update events so only the latest is kept
            var archiveEvent = Model.CreatedEarningsProfileUpdatedEvent();
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
        return Model.AdditionalPayments
            .Where(x=> x.AdditionalPaymentType == InstalmentTypes.LearningSupport)
            .Select((AdditionalPayment.Get))
            .ToList().AsReadOnly();
    }

    /// <summary>
    /// Maths and English courses are not calculated, but are instead added to the earnings profile via an external process.
    /// In the event of a recalculation, these should be preserved.
    /// This method returns the courses so that they can be preserved.
    /// </summary>
    public IReadOnlyCollection<MathsAndEnglish> PersistentMathsAndEnglishCourses()
    {
        if (Model.MathsAndEnglishCourses == null)
            return new List<MathsAndEnglish>();

        return Model.MathsAndEnglishCourses
            .Select((MathsAndEnglish.Get))
            .ToList().AsReadOnly();
    }

    public EarningsProfileModel GetModel()
    {
        return Model;
    }

    public static EarningsProfile Get(ApprenticeshipEpisode episode, EarningsProfileModel model)
    {
        var profile =  new EarningsProfile(model, episode.AddChildToRoot);
        return profile;
    }

    private void UpdateEnglishAndMathsCourses(List<MathsAndEnglish> updatedCourses)
    {
        Model.MathsAndEnglishCourses ??= new List<MathsAndEnglishModel>();

        // 1. If nothing remains, clear everything
        if (updatedCourses.Count == 0)
        {
            Model.MathsAndEnglishCourses.Clear();
            return;
        }

        // 2. Sync courses by (LearnAimRef, StartDate)
        Model.MathsAndEnglishCourses.SyncByKey(
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
                    createNew: up => up.GetModel()
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
                    createNew: up => up.GetModel()
                );
            },
            createNew: updated => updated.GetModel()
        );
    }
}