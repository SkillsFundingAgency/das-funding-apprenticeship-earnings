using SFA.DAS.Apprenticeships.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Helpers;

public class ApprenticeshipWithdrawnEventBuilder
{
    private Guid _apprenticeshipKey = Guid.NewGuid();
    private long _apprenticeshipId = 123;
    private string _reason = "Withdrawal test";
    private DateTime _lastDayOfLearning = new DateTime(2020, 08, 31);

    public ApprenticeshipWithdrawnEventBuilder WithApprenticeshipKey(Guid key)
    {
        _apprenticeshipKey = key;
        return this;
    }

    public ApprenticeshipWithdrawnEventBuilder WithApprenticeshipId(long id)
    {
        _apprenticeshipId = id;
        return this;
    }

    public ApprenticeshipWithdrawnEventBuilder WithReason(string reason)
    {
        _reason = reason;
        return this;
    }

    public ApprenticeshipWithdrawnEventBuilder WithLastDayOfLearning(DateTime date)
    {
        _lastDayOfLearning = date;
        return this;
    }

    public ApprenticeshipWithdrawnEventBuilder WithExistingApprenticeshipData(ApprenticeshipCreatedEvent apprenticeship)
    {
        _apprenticeshipKey = apprenticeship.ApprenticeshipKey;
        _apprenticeshipId = apprenticeship.ApprovalsApprenticeshipId;
        return this;
    }

    public ApprenticeshipWithdrawnEvent Build()
    {
        return new ApprenticeshipWithdrawnEvent
        {
            ApprenticeshipKey = _apprenticeshipKey,
            ApprenticeshipId = _apprenticeshipId,
            Reason = _reason,
            LastDayOfLearning = _lastDayOfLearning
        };
    }
}