namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.EnglishAndMaths;

/// <summary>
/// Keys of the English and Maths courses affected by the most recent update to an earnings profile.
/// </summary>
public class EnglishAndMathsCourseChanges
{
    public List<Guid> Created { get; } = [];
    public List<Guid> Changed { get; } = [];
    public List<Guid> Removed { get; } = [];
    public List<Guid> Reinstated { get; } = [];

    public bool HasChanges => Created.Any() || Changed.Any() || Removed.Any() || Reinstated.Any();
}
