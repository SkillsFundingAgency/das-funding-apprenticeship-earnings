namespace SFA.DAS.Funding.ApprenticeshipEarnings.Types;

public class EarningsProfileUpdatedEvent
{
    public Guid EarningsProfileId { get; set; }
    public Guid EpisodeKey { get; set; }
    public Guid Version { get; set; }
    public decimal OnProgramTotal { get; set; }
    public decimal CompletionPayment { get; set; }
    public List<Instalment> Instalments { get; set; } = null!;
    public List<AdditionalPayment> AdditionalPayments { get; set; } = null!;
    public List<EnglishAndMaths> EnglishAndMaths { get; set; } = null!;
    public bool InitialGeneration { get; set; }
}

public class EnglishAndMaths
{
    public Guid EnglishAndMathsKey { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Course { get; set; } = null!;
    public decimal Amount { get; set; }
    public List<EnglishAndMathsInstalments> Instalments { get; set; } = [];
}

public class EnglishAndMathsInstalments
{
    public short AcademicYear { get; set; }
    public byte DeliveryPeriod { get; set; }
    public decimal Amount { get; set; }
}

public class AdditionalPayment
{
    public Guid Key { get; set; }
    public string AdditionalPaymentType { get; set; }
    public DateTime DueDate { get; set; }
    public short AcademicYear { get; set; }
    public byte DeliveryPeriod { get; set; }
    public decimal Amount { get; set; }
    public bool IsAfterLearningEnded { get; set; }
}

public class Instalment
{
    public Guid Key { get; set; }
    public Guid EpisodePriceKey { get; set; }
    public string Type { get; set; }
    public short AcademicYear { get; set; }
    public byte DeliveryPeriod { get; set; }
    public decimal Amount { get; set; }
    public bool IsAfterLearningEnded { get; set; }
}