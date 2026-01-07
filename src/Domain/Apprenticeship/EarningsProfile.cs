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

    public EarningsProfile(
        decimal onProgramTotal,
        List<Instalment> instalments,
        List<AdditionalPayment> additionalPayments,
        List<MathsAndEnglish> mathsAndEnglishCourses,
        decimal completionPayment,
        Guid episodeKey, Action<AggregateComponent> addChildToRoot) : base(addChildToRoot)
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

    public void Update(
        ISystemClockService systemClock,
        decimal? onProgramTotal = null, 
        List<Instalment>? instalments = null, 
        List<AdditionalPayment>? additionalPayments = null, 
        List<MathsAndEnglish>? mathsAndEnglishCourses = null,
        decimal? completionPayment = null
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

        // --- 1. If no courses remain, delete everything ---
        if (updatedCourses.Count == 0)
        {
            Model.MathsAndEnglishCourses.Clear();
            return;
        }

        // --- 2. Process each updated course ---
        foreach (var updatedCourse in updatedCourses)
        {
            var existingCourse = Model.MathsAndEnglishCourses
                .FirstOrDefault(c => c.StartDate == updatedCourse.StartDate
                                      && c.LearnAimRef == updatedCourse.LearnAimRef);

            if (existingCourse == null)
            {
                // Entirely new course – add whole graph
                Model.MathsAndEnglishCourses.Add(updatedCourse.GetModel());
                continue;
            }

            // Update course-level fields
            existingCourse.StartDate = updatedCourse.StartDate;
            existingCourse.EndDate = updatedCourse.EndDate;
            existingCourse.Amount = updatedCourse.Amount;
            existingCourse.WithdrawalDate = updatedCourse.WithdrawalDate;
            existingCourse.ActualEndDate = updatedCourse.ActualEndDate;
            existingCourse.PauseDate = updatedCourse.PauseDate;
            existingCourse.PriorLearningAdjustmentPercentage = updatedCourse.PriorLearningAdjustmentPercentage;

            // --- 3. Sync instalments (hard delete) ---

            // Identity = stable fields only
            bool Matches(MathsAndEnglishInstalmentModel existing, MathsAndEnglishInstalment updated) =>
                existing.AcademicYear == updated.AcademicYear &&
                existing.DeliveryPeriod == updated.DeliveryPeriod &&
                existing.Type == updated.Type.ToString();

            // 3a. Remove instalments that no longer exist
            existingCourse.Instalments.RemoveAll(existing =>
                !updatedCourse.Instalments.Any(updated => Matches(existing, updated)));

            // 3b. Add or update instalments
            foreach (var updatedInstalment in updatedCourse.Instalments)
            {
                var existingInstalment = existingCourse.Instalments
                    .FirstOrDefault(e => Matches(e, updatedInstalment));

                if (existingInstalment == null)
                {
                    // New instalment
                    existingCourse.Instalments.Add(updatedInstalment.GetModel());
                }
                else
                {
                    // Update mutable fields
                    existingInstalment.Amount = updatedInstalment.Amount;
                }
            }
        }

        // --- 4. Remove courses that no longer exist ---
        var updatedCourseKeys = updatedCourses
            .Select(c => c.Course)
            .ToHashSet();

        Model.MathsAndEnglishCourses.RemoveAll(c => !updatedCourseKeys.Contains(c.Course));
    }
}