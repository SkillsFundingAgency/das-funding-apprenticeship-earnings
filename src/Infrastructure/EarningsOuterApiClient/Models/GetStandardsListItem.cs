using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.EarningsOuterApiClient.Models;

#pragma warning disable CS8618
[ExcludeFromCodeCoverage]
public class GetStandardsListItem : StandardApiResponseBase
{
    public string StandardUId { get; set; }
    public string IfateReferenceNumber { get; set; }
    public int LarsCode { get; set; }
    public string Status { get; set; }
    public string Title { get; set; }
    public int Level { get; set; }
    public string Version { get; set; }
    public int VersionMajor { get; set; }
    public int VersionMinor { get; set; }
    public List<string> Options { get; set; }
    public StandardVersionDetail VersionDetail { get; set; }
    public string StandardPageUrl { get; set; }
}
#pragma warning restore CS8618