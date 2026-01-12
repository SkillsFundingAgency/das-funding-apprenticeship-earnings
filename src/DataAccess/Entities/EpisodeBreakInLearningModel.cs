//using System.ComponentModel.DataAnnotations.Schema;

//namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;

//[Dapper.Contrib.Extensions.Table("Domain.EpisodeBreakInLearning")]
//[Table("EpisodeBreakInLearning", Schema = "Domain")]
//public class EpisodeBreakInLearningModel
//{
//    [Dapper.Contrib.Extensions.Key]
//    [DatabaseGenerated(DatabaseGeneratedOption.None)]
//    public Guid Key { get; set; }
//    public Guid EpisodeKey { get; set; }
//    public DateTime StartDate { get; set; }
//    public DateTime EndDate { get; set; }
//    public DateTime PriorPeriodExpectedEndDate { get; set; }
//}