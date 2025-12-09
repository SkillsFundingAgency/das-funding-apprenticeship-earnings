using AutoFixture;
using Moq;
using NServiceBus;
using SFA.DAS.Learning.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.ProcessWithdrawnApprenticeshipCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.ProcessWithdrawnApprenticeshipsCommandHandler;

[TestFixture]
public class WhenProcessWithdrawnApprenticeshipCommandHandled
{
    private readonly Fixture _fixture = new();
    private readonly Mock<ISystemClockService> _mockSystemClock = new();
    private readonly Mock<IApprenticeshipRepository> _mockRepository = new();

    private void SetupMocks()
    {
        _mockRepository.Reset();
        _mockSystemClock.Setup(x => x.UtcNow).Returns(new DateTime(2024, 12, 1));
    }

    [Test]
    public async Task ThenTheApprenticeshipIsWithdrawn()
    {
        // Arrange
        var apprenticeshipModel = _fixture.BuildApprenticeshipModel();
        apprenticeshipModel.Episodes.ForEach(x => x.EarningsProfile.Instalments.ForEach(y => y.Type = InstalmentType.Regular.ToString()));
        var apprenticeship = Apprenticeship.Get(apprenticeshipModel);
        SetupMocks();
        var command = BuildCommand(apprenticeship);

        _mockRepository
            .Setup(x => x.Get(command.ApprenticeshipKey))
            .ReturnsAsync(apprenticeship);

        var sut = new ProcessWithdrawnApprenticeshipCommandHandler(
            _mockRepository.Object,
            _mockSystemClock.Object
        );

        // Act
        await sut.Handle(command);

        // Assert
        _mockRepository.Verify(x => x.Get(command.ApprenticeshipKey), Times.Once);
        _mockRepository.Verify(x => x.Update(It.IsAny<Apprenticeship>()), Times.Once);
    }

    private ProcessWithdrawnApprenticeshipCommand.ProcessWithdrawnApprenticeshipCommand BuildCommand(Apprenticeship apprenticeship)
    {
        var withdrawRequest = new WithdrawRequest
        {
            WithdrawalDate = new DateTime(2024, 11, 30)
        };

        return new ProcessWithdrawnApprenticeshipCommand.ProcessWithdrawnApprenticeshipCommand(apprenticeship.ApprenticeshipKey, withdrawRequest);
    }
}