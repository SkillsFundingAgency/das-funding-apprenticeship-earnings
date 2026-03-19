using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Model;

internal class UpdateShortCourseOnProgrammeModel
{
    public TrackedValue<DateTime?> WithdrawalDate { get; set; } = new TrackedValue<DateTime?>(null);
    public TrackedValue<List<Milestone>> Milestones { get; set; } = new TrackedValue<List<Milestone>>(new List<Milestone>());
}
