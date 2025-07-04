using SFA.DAS.Learning.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Helpers;

public class LearningWithdrawnEventBuilder
{
    private Guid _learningKey = Guid.NewGuid();
    private long _apprenticeshipId = 123;
    private string _reason = "Withdrawal test";
    private DateTime _lastDayOfLearning = new DateTime(2020, 08, 31);

    public LearningWithdrawnEventBuilder WithLearningKey(Guid key)
    {
        _learningKey = key;
        return this;
    }

    public LearningWithdrawnEventBuilder WithApprenticeshipId(long id)
    {
        _apprenticeshipId = id;
        return this;
    }

    public LearningWithdrawnEventBuilder WithReason(string reason)
    {
        _reason = reason;
        return this;
    }

    public LearningWithdrawnEventBuilder WithLastDayOfLearning(DateTime date)
    {
        _lastDayOfLearning = date;
        return this;
    }

    public LearningWithdrawnEventBuilder WithExistingApprenticeshipData(LearningCreatedEvent apprenticeship)
    {
        _learningKey = apprenticeship.LearningKey;
        _apprenticeshipId = apprenticeship.ApprovalsApprenticeshipId;
        return this;
    }

    public LearningWithdrawnEvent Build()
    {
        return new LearningWithdrawnEvent
        {
            LearningKey = _learningKey,
            ApprovalsApprenticeshipId = _apprenticeshipId,
            Reason = _reason,
            LastDayOfLearning = _lastDayOfLearning
        };
    }
}