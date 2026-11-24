namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.EnglishAndMaths;

/// <summary>
/// The withdrawal and completion dates an English and Maths course is earned against, once any withdrawal of the
/// apprenticeship it belongs to has been taken into account.
/// </summary>
public record EnglishAndMathsEndDates(DateTime? WithdrawalDate, DateTime? CompletionDate);
