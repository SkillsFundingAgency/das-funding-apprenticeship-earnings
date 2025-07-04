
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

public class AcademicYearService : IAcademicYearService
{
    private readonly IDateService _dateService;

    public AcademicYearService(IDateService dateService)
    {
        _dateService = dateService;
    }

    public short CurrentAcademicYear => _dateService.Today.ToAcademicYear();

    public DateTime StartOfCurrentAcademicYear(DateTime currentDate)
    {

        if (currentDate.Month >= 8)
        {
            return new DateTime(currentDate.Year, 8, 1);
        }
        else
        {
            return new DateTime(currentDate.Year - 1, 8, 1);
        }

    }

    public DateTime EndOfCurrentAcademicYear(DateTime currentDate)
    {

        if (currentDate.Month >= 8)
        {
            return new DateTime(currentDate.Year + 1, 7, 31);
        }
        else
        {
            return new DateTime(currentDate.Year, 7, 31);
        }
    }
}
