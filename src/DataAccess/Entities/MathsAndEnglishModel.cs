using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;

[Dapper.Contrib.Extensions.Table("Domain.MathsAndEnglish")]
[Table("MathsAndEnglish", Schema = "Domain")]
public class MathsAndEnglishModel
{
    public MathsAndEnglishModel() { }

    public MathsAndEnglishModel(Guid earningsProfileId, string course, string learnAimRef, DateTime startDate, DateTime endDate, decimal amount, DateTime? withdrawalDate, DateTime? actualEndDate, int? priorLearningAdjustmentPercentage)
    {
        Key = Guid.NewGuid();
        EarningsProfileId = earningsProfileId;
        Course = course;
        LearnAimRef = learnAimRef;
        StartDate = startDate;
        EndDate = endDate;
        Amount = amount;
        WithdrawalDate = withdrawalDate;
        ActualEndDate = actualEndDate;
        PriorLearningAdjustmentPercentage = priorLearningAdjustmentPercentage;
    }

    [Dapper.Contrib.Extensions.Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Key { get; set; }

    public Guid EarningsProfileId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string Course { get; set; } = null!;
    public string LearnAimRef { get; set; } = null!;

    public decimal Amount { get; set; }

    public DateTime? WithdrawalDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public DateTime? PauseDate { get; set; }
    public int? PriorLearningAdjustmentPercentage { get; set; }

    public List<MathsAndEnglishInstalmentModel> Instalments { get; set; } = [];

}