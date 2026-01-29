using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Model;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateEnglishAndMathsCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateOnProgrammeCommand;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Helpers;

public class UpdateEnglishAndMathsRequestBuilder
{
    private DateTime _startDate = new DateTime(2020, 02, 01);
    private DateTime _endDate = new DateTime(2022, 1, 1);
    private string _course = "Maths 4";
    private string _learnAimRef = "4A";
    private decimal _amount = 4000;
    private DateTime? _withdrawalDate = null;
    private int? _priorLearningAdjustmentPercentage = null;
    private DateTime? _completionDate = null;
    private DateTime? _pauseDate = null;
    private List<PeriodInLearningItem> _periodsInLearning =
        [ new() { StartDate = new DateTime(2020, 02, 01), EndDate = new DateTime(2022, 1, 1), OriginalExpectedEndDate = new DateTime(2022, 1, 1) } ];

    public UpdateEnglishAndMathsRequestBuilder WithDataFromSetupModel(UpdateEnglishAndMathsModel model)
    {
        if (model.StartDate.HasChanged)
            _startDate = model.StartDate.Value;

        if (model.EndDate.HasChanged)
            _endDate = model.EndDate.Value;

        if (model.Course.HasChanged)
            _course = model.Course.Value;

        if (model.LearnAimRef.HasChanged)
            _learnAimRef = model.LearnAimRef.Value;

        if (model.Amount.HasChanged)
            _amount = model.Amount.Value;

        if (model.WithdrawalDate.HasChanged)
            _withdrawalDate = model.WithdrawalDate.Value;

        if (model.PriorLearningAdjustmentPercentage.HasChanged)
            _priorLearningAdjustmentPercentage = model.PriorLearningAdjustmentPercentage.Value;

        if (model.CompletionDate.HasChanged)
            _completionDate = model.CompletionDate.Value;

        if (model.PauseDate.HasChanged)
            _pauseDate = model.PauseDate.Value;

        if (model.PeriodsInLearning.HasChanged)
        {
            _periodsInLearning = model.PeriodsInLearning.Value;
        }
        else
        {
            // Default to a single period spanning the English & Maths dates
            _periodsInLearning =
            [
                new PeriodInLearningItem
                {
                    StartDate = _startDate,
                    EndDate = _endDate,
                    OriginalExpectedEndDate = _endDate
                }
            ];
        }

        return this;
    }

    public UpdateEnglishAndMathsRequest Build()
    {
        return new UpdateEnglishAndMathsRequest
        {
            EnglishAndMaths =
            [
                new EnglishAndMathsItem
                {
                    StartDate = _startDate,
                    EndDate = _endDate,
                    Course = _course,
                    LearnAimRef = _learnAimRef,
                    Amount = _amount,
                    WithdrawalDate = _withdrawalDate,
                    PriorLearningAdjustmentPercentage = _priorLearningAdjustmentPercentage,
                    CompletionDate = _completionDate,
                    PauseDate = _pauseDate,
                    PeriodsInLearning = _periodsInLearning
                }
            ]
        };
    }
}