using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain
{
    public class EarningsProfile
    {
        public decimal? AdjustedPrice { get; set; }
        public List<EarningsInstallment> Installments { get; set; }
    }

    public class EarningsInstallment
    {
        public short AcademicYear { get; set; } 
        public byte DeliveryPeriod { get; set; }
        public decimal Amount { get; set; }
    }
}
