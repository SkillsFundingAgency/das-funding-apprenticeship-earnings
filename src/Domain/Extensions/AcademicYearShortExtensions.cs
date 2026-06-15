namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;

public static class AcademicYearShortExtensions
{
    public static (DateTime StartDate, DateTime EndDate) GetAcademicYear(this short academicYear)
    {
        var academicYearString = academicYear.ToString("D4");
        var startYear = 2000 + int.Parse(academicYearString[..2]);
        var endYear = startYear + 1;

        return (new DateTime(startYear, 8, 1), new DateTime(endYear, 7, 31));
    }
}
