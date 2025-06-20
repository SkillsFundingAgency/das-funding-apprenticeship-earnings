﻿namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Model;

public class PriceChangeModel
{
    public DateTime? EffectiveFromDate { get; set; }
    public DateTime? ChangeRequestDate{ get; set; }
    public decimal? NewTrainingPrice { get; set; }
    public decimal? NewAssessmentPrice { get; set; }
}