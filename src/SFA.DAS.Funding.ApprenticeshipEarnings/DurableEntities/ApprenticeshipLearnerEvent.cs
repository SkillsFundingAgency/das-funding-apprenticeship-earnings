using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities
{
    //todo this obviously needs to come from the event that will be published by the upstream service (Apprenticeship Earnings Approvals Event Handlers or similar)
    [Obsolete("this obviously needs to come from the event that will be published by the upstream service (Apprenticeship Earnings Approvals Event Handlers or similar)")]
    public class InternalApprenticeshipLearnerEvent
    {
        public string DummyEventData { get; set; }
    }
}
