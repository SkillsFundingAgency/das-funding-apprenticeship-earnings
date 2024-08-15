namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.ApprenticeshipFunding;

public class ApprenticeshipFunding
{
    private const decimal AgreedPriceMultiplier = 0.8m;
    public decimal OnProgramTotal { get; }
    public decimal CompletionPayment { get; }

    public ApprenticeshipFunding(decimal agreedPrice, decimal fundingBandMaximum)
    {
        var cappedAgreedPrice = CalculateCappedAgreedPrice(fundingBandMaximum, agreedPrice);
        OnProgramTotal = CalculateOnProgramTotalAmount(cappedAgreedPrice);
        CompletionPayment = CalculateCompletionPayment(cappedAgreedPrice);
    }
    
    private static decimal CalculateCappedAgreedPrice(decimal fundingBandMaximum, decimal agreedPrice)
    {
        return Math.Min(fundingBandMaximum, agreedPrice);
    }

    private decimal CalculateCompletionPayment(decimal agreedPrice)
    {
        return agreedPrice - OnProgramTotal;
    }

    private static decimal CalculateOnProgramTotalAmount(decimal agreedPrice)
    {
        return agreedPrice * AgreedPriceMultiplier;
    }
}