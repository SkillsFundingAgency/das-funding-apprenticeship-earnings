using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using System.Linq;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.BreakInLearningCommand;

public class BreaksInLearningCommandHandler : ICommandHandler<BreaksInLearningCommand>
{

    private readonly IApprenticeshipRepository _apprenticeshipRepository;
    private readonly ISystemClockService _systemClock;

    public BreaksInLearningCommandHandler(IApprenticeshipRepository apprenticeshipRepository, ISystemClockService systemClock)
    {
        _apprenticeshipRepository = apprenticeshipRepository;
        _systemClock = systemClock;
    }

    public async Task Handle(BreaksInLearningCommand command, CancellationToken cancellationToken = default)
    {
        var apprenticeshipDomainModel = await _apprenticeshipRepository.Get(command.ApprenticeshipKey);
        var episode = apprenticeshipDomainModel.GetEpisode(command.EpisodeKey);

        var breaksInLearning = command.BreaksInLearning
            .Select(b => new EpisodeBreakInLearning(command.EpisodeKey, b.StartDate, b.EndDate))
            .ToList();


        episode.UpdateBreaksInLearning(breaksInLearning);
        apprenticeshipDomainModel.Calculate(_systemClock, command.EpisodeKey);

        try
        {
            await _apprenticeshipRepository.Update(apprenticeshipDomainModel);
        }
        catch(Exception ex)
        {
            // Log exception or handle it as needed
            throw;
        }

    }
}
