using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship.Events;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using System.Collections.ObjectModel;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

public class EarningsProfile<T> : AggregateComponent where T : EarningsProfileModel, new()
{
    protected T Model;

    public EarningsProfile():base(null) { }

    public EarningsProfile(T model, Action<AggregateComponent> addChildToRoot) : base(addChildToRoot)
    {
        Model = model;
    }

    public T GetModel()
    {
        return Model;
    }
}

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
        var historyEntity = new EarningsProfileHistoryModel(Model, systemClock.UtcNow.Date);// this needs to be created before any changes, although it will be discarded if none are made
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
            Model.MathsAndEnglishCourses = mathsAndEnglishCourses!.ToModels<MathsAndEnglish, MathsAndEnglishModel>();
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
            AddEvent(new EarningsProfileArchivedEvent(historyEntity));
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
}