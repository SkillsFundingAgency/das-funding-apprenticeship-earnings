using System.Text.Json.Serialization;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.ReleaseEarningsCommand;

public class ReleaseEarningsRequest
{
    public Guid LearnerKey { get; set; }
    public string LearnerRef { get; set; } = string.Empty;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ReleaseType ReleaseType { get; set; } = ReleaseType.All;
    public List<Guid> EnglishAndMathsCourseKeys { get; set; } = [];
}
