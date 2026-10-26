using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.ApplyFurtherApprovalCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.ApplyFurtherApprovalCommandHandler;

[TestFixture]
public class WhenApplyingFurtherApproval
{
    private Fixture _fixture = null!;
    private Mock<ILearningRepository> _learningRepository = null!;
    private Command.ApplyFurtherApprovalCommand.ApplyFurtherApprovalCommandHandler _handler = null!;
    private ApprenticeshipLearning _learning = null!;
    private ApprenticeshipEpisode _episode = null!;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _learningRepository = new Mock<ILearningRepository>();
        _handler = new Command.ApplyFurtherApprovalCommand.ApplyFurtherApprovalCommandHandler(
            _learningRepository.Object,
            Mock.Of<ILogger<Command.ApplyFurtherApprovalCommand.ApplyFurtherApprovalCommandHandler>>());

        var model = _fixture.BuildLearningModel();
        model.Episodes.Single().FurtherApprovalNeeded = true;
        _learning = ApprenticeshipLearning.Get(model);
        _episode = _learning.Episodes.Single();

        _learningRepository.Setup(x => x.GetApprenticeshipLearning(_learning.LearningKey)).ReturnsAsync(_learning);
    }

    [Test]
    public async Task ThenTheApprovalIsAppliedAndTheLearningIsSaved()
    {
        var command = new ApplyFurtherApprovalCommand.ApplyFurtherApprovalCommand(_learning.LearningKey, _episode.EpisodeKey, Guid.NewGuid(), "LEARNER-REF");

        await _handler.Handle(command);

        _episode.FurtherApprovalNeeded.Should().BeFalse();
        _learning.FlushEvents().OfType<ApprenticeshipPayableEarningsUpdatedEvent>().Should().ContainSingle(x =>
            x.LearnerKey == command.LearnerKey && x.LearnerRef == command.LearnerRef);
        _learningRepository.Verify(x => x.Update(_learning), Times.Once);
    }

    [Test]
    public async Task AndTheLearningIsNotFoundThenNothingIsSaved()
    {
        var command = new ApplyFurtherApprovalCommand.ApplyFurtherApprovalCommand(Guid.NewGuid(), _episode.EpisodeKey, Guid.NewGuid(), "LEARNER-REF");

        await _handler.Handle(command);

        _learningRepository.Verify(x => x.Update(It.IsAny<ApprenticeshipLearning>()), Times.Never);
    }

    [Test]
    public async Task AndTheEpisodeIsNotFoundThenNothingIsSaved()
    {
        var command = new ApplyFurtherApprovalCommand.ApplyFurtherApprovalCommand(_learning.LearningKey, Guid.NewGuid(), Guid.NewGuid(), "LEARNER-REF");

        await _handler.Handle(command);

        _episode.FurtherApprovalNeeded.Should().BeTrue();
        _learningRepository.Verify(x => x.Update(It.IsAny<ApprenticeshipLearning>()), Times.Never);
    }
}
