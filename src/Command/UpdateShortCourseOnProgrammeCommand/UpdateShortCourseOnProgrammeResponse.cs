using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.ShortCourse;
using ShortCourseLearningDomainModel = SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.ShortCourse.ShortCourseLearning;
using ShortCourseEpisodeDomainModel = SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.ShortCourse.ShortCourseEpisode;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateShortCourseOnProgrammeCommand;

public class UpdateShortCourseOnProgrammeResponse : SFA.DAS.Funding.ApprenticeshipEarnings.DataTransferObjects.ShortCourseEarnings
{
}

public static class UpdateShortCourseOnProgrammeResponseMapper
{
    public static UpdateShortCourseOnProgrammeResponse ToResponse(ShortCourseLearningDomainModel learning)
    {
        var episode = learning.GetEpisode();
        var earningsProfile = episode.EarningsProfile!;

        var response = new UpdateShortCourseOnProgrammeResponse
        {
            EarningProfileVersion = earningsProfile.Version,
            Instalments = earningsProfile.Instalments.Select(i => MapToInstalment(episode, i)).ToList()
        };

        return response;
    }

    private static DataTransferObjects.ShortCourseInstalment MapToInstalment(ShortCourseEpisodeDomainModel episode, ShortCourseInstalment instalment)
    {
        var instalmentType = instalment.Type.ToString();
        var milestoneFlag = Enum.Parse<MilestoneFlags>(instalmentType);

        return new DataTransferObjects.ShortCourseInstalment
        {
            Amount = instalment.Amount,
            CollectionYear = instalment.AcademicYear,
            CollectionPeriod = instalment.DeliveryPeriod,
            Type = instalmentType,
            IsPayable = instalment.IsPayable
        };
    }
}
