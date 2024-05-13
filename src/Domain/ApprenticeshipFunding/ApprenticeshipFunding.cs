namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.ApprenticeshipFunding;

public class ApprenticeshipFunding
{
    private const decimal AgreedPriceMultiplier = 0.8m;
    private readonly DateTime _startDate;
    private readonly DateTime _endDate;
    public decimal OnProgramTotal { get; }
    public decimal CompletionPayment { get; }
    public decimal CappedAgreedPrice { get; }

    public ApprenticeshipFunding(decimal agreedPrice, DateTime startDate, DateTime endDate, decimal fundingBandMaximum)
    {
        _startDate = startDate;
        _endDate = endDate;
        CappedAgreedPrice = CalculateCappedAgreedPrice(fundingBandMaximum, agreedPrice);
        OnProgramTotal = CalculateOnProgramTotalAmount(CappedAgreedPrice);
        CompletionPayment = CalculateCompletionPayment(CappedAgreedPrice);
    }

    private decimal CalculateCappedAgreedPrice(decimal fundingBandMaximum, decimal agreedPrice)
    {
        return Math.Min(fundingBandMaximum, agreedPrice);
    }

    private decimal CalculateCompletionPayment(decimal agreedPrice)
    {
        return agreedPrice - OnProgramTotal;
    }

    private decimal CalculateOnProgramTotalAmount(decimal agreedPrice)
    {
        return agreedPrice * AgreedPriceMultiplier;
    }

    //TODO currently have 5/6 different classes with a "RecalculateEarnings" method that don't actually do anything apart from pass the responsibility down to a lower layer
    //- simplify the ApprenticeshipFunding & InstalmentsGenerator classes into one class?
    public List<Earning> GenerateEarnings()
    {
        var instalmentGenerator = new InstalmentsGenerator();
        var earnings = instalmentGenerator.Generate(OnProgramTotal, _startDate, _endDate);
        return earnings;
    }

    public List<Earning> RecalculateEarnings(List<Earning> existingEarnings, DateTime effectivePriceChangeDate)
    {
        var installmentGenerator = new InstalmentsGenerator();
        var earnings = installmentGenerator.Recalculate(OnProgramTotal, effectivePriceChangeDate, _endDate, existingEarnings);
        return earnings;
    }

    public List<Earning> RecalculateEarnings(DateTime newStartDate)
    {
        var installmentGenerator = new InstalmentsGenerator();
        var earnings = installmentGenerator.Generate(OnProgramTotal, newStartDate, _endDate);
        return earnings;
    }
}