namespace SFA.DAS.Funding.ApprenticeshipEarnings.Types;

public class ArchiveEarningsProfileEvent
{
    public Guid EarningsProfileId { get; set; }
    public Guid EpisodeKey { get; set; }
    public Guid Version { get; set; }
    public decimal OnProgramTotal { get; set; }
    public decimal CompletionPayment { get; set; }
    public List<Instalment> Instalments { get; set; } = null!;
    public List<AdditionalPayment> AdditionalPayments { get; set; } = null!;
    public List<MathsAndEnglish> MathsAndEnglishCourses { get; set; } = null!;
    public DateTime SupersededDate { get; set; }
}

public abstract class ArchiveEntity
{
    public Guid Key { get; set; }
    public Guid? EarningsProfileVersion { get; set; }
}

public abstract class PaymentEntity : ArchiveEntity
{
    public short AcademicYear { get; set; }
    public byte DeliveryPeriod { get; set; }
    public decimal Amount { get; set; }
}

public class MathsAndEnglish : ArchiveEntity
{
    public Guid EarningsProfileId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string Course { get; set; } = null!;

    public decimal Amount { get; set; }

    public List<MathsAndEnglishInstalment> Instalments { get; set; } = [];
}

public class MathsAndEnglishInstalment : PaymentEntity
{
    public Guid MathsAndEnglishKey { get; set; }
}

public class AdditionalPayment : PaymentEntity
{
    public Guid EarningsProfileId { get; set; }
    public string AdditionalPaymentType { get; set; }
    public DateTime DueDate { get; set; }
}

public class Instalment : PaymentEntity
{
    public Guid EarningsProfileId { get; set; }
    public Guid EpisodePriceKey { get; set; }
}