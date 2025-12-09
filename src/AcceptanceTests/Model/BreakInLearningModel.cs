using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Model;

public class BreakInLearningModel
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime PreviousPeriodExpectedEndDate { get; internal set; }
}
