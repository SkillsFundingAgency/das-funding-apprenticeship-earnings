using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Model;

public class ShortCourseUpdateResponseExpectationModel
{
    public Decimal Amount { get; set; }
    public short CollectionYear { get; set; }
    public byte CollectionPeriod { get; set; }
    public string Type { get; set; } = string.Empty;
    public bool IsPayable { get; set; }
}
