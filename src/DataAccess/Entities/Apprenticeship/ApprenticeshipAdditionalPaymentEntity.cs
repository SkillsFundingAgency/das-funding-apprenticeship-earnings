using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.Apprenticeship;

[Dapper.Contrib.Extensions.Table("Domain.ApprenticeshipAdditionalPayment")]
[Table("ApprenticeshipAdditionalPayment", Schema = "Domain")]
public class ApprenticeshipAdditionalPaymentEntity : BaseAdditionalPaymentEntity
{

}
