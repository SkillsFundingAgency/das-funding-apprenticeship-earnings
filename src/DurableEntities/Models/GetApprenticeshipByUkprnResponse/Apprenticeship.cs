using System;
using System.Collections.Generic;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models.GetApprenticeshipByUkprnResponse;

public class Apprenticeship
{
    public long Ukprn { get; set; }
    public Guid Key { get; set; } = Guid.Empty;
    public List<Episode> Episodes { get; set; }
    public string FundingLineType { get; set; }
}

public class Episode
{
    public Guid Key { get; set; }
    public int NumberOfInstalments { get; set; }
    public List<Instalment> Instalments { get; set; }
    public decimal CompletionPayment { get; set; }
    public decimal OnProgramTotal { get; set; }

}

public class Instalment
{
    public short AcademicYear { get; set; }
    public byte DeliveryPeriod { get; set; }
    public decimal Amount { get; set; }
}