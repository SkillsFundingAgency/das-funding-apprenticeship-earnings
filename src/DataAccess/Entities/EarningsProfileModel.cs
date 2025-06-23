using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;

public abstract class EarningsProfileModelBase
{
    [Dapper.Contrib.Extensions.Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid EarningsProfileId { get; set; }
    public Guid EpisodeKey { get; set; }
    public Guid Version { get; set; }
    public decimal OnProgramTotal { get; set; }
    public decimal CompletionPayment { get; set; }

    public EarningsProfileModelBase() { }

    public EarningsProfileModelBase(EarningsProfileModelBase original)
    {
        EarningsProfileId = Guid.NewGuid();
        EpisodeKey = original.EpisodeKey;
        CompletionPayment = original.CompletionPayment;
        OnProgramTotal = original.OnProgramTotal;
        Version = original.Version;
    }
}

[Dapper.Contrib.Extensions.Table("Domain.EarningsProfile")]
[Table("EarningsProfile", Schema = "Domain")]
public class EarningsProfileModel : EarningsProfileModelBase
{
    public List<InstalmentModel> Instalments { get; set; } = null!;
    public List<AdditionalPaymentModel> AdditionalPayments { get; set; } = null!;
    public List<MathsAndEnglishModel> MathsAndEnglishCourses { get; set; } = null!;
}

[Dapper.Contrib.Extensions.Table("Domain.EarningsProfileHistory")]
[Table("EarningsProfileHistory", Schema = "Domain")]
public class EarningsProfileHistoryModel : EarningsProfileModelBase
{
    public EarningsProfileHistoryModel(EarningsProfileModel original, DateTime supersededDate) : base(original)
    {
        OriginalEarningsProfileId = original.EarningsProfileId;
        SupersededDate = supersededDate;
        Instalments = original.Instalments.Select(x => new InstalmentHistoryModel(x, EarningsProfileId, original.Version)).ToList();
        AdditionalPayments = original.AdditionalPayments
            .Select(x => new AdditionalPaymentHistoryModel(x, EarningsProfileId)).ToList();
    }

    public EarningsProfileHistoryModel() {}

    public List<InstalmentHistoryModel> Instalments { get; set; } = null!;
    public List<AdditionalPaymentHistoryModel> AdditionalPayments { get; set; } = null!;
    public List<MathsAndEnglishModel> MathsAndEnglishCourses { get; set; } = null!;
    public DateTime SupersededDate { get; set; }
    public Guid OriginalEarningsProfileId { get; set; }
}