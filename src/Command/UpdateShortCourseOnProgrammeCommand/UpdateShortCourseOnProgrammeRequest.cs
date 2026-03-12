using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateShortCourseOnProgrammeCommand;

#pragma warning disable CS8618 

public class UpdateShortCourseOnProgrammeRequest
{
    public DateTime? WithdrawalDate { get; set; }
    public List<Milestone> Milestones { get; set; }
}

#pragma warning restore CS8618
