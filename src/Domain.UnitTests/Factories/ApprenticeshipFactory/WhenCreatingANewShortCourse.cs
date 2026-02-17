using System;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using SFA.DAS.Learning.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.Factories.ApprenticeshipFactory;

[TestFixture]
public class WhenCreatingANewShortCourse
{
    private Fixture _fixture;
    private Domain.Factories.ApprenticeshipFactory _factory;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _factory = new Domain.Factories.ApprenticeshipFactory();
    }

    [Test]
    public void ThenTheShortCourseIsCreatedCorrectly()
    {
        // Arrange
        var request = new CreateUnapprovedShortCourseLearningRequest
        {
            LearningKey = Guid.NewGuid(),
            Learner = new Learner
            {
                DateOfBirth = new DateTime(2000, 01, 01),
                Uln = "1234567890"
            },
            OnProgramme = new OnProgramme
            {
                EmployerId = 987654,
                CourseCode = "SC101",
                StartDate = new DateTime(2025, 01, 01),
                ExpectedEndDate = new DateTime(2025, 06, 30),
                TotalPrice = 1500m
            }
        };

        // Act
        var apprenticeship = _factory.CreateNewShortCourse(request);

        // Assert
        apprenticeship.ApprenticeshipKey.Should().Be(request.LearningKey);
        apprenticeship.DateOfBirth.Should().Be(request.Learner.DateOfBirth);
        apprenticeship.Uln.Should().Be(request.Learner.Uln);

        var episode = apprenticeship.ApprenticeshipEpisodes.SingleOrDefault();
        episode.Should().NotBeNull();
        episode.EmployerAccountId.Should().Be(request.OnProgramme.EmployerId);
        episode.TrainingCode.Should().Be(request.OnProgramme.CourseCode);
        episode.AgeAtStartOfApprenticeship.Should().Be(25); // 2025 - 2000
        episode.FundingBandMaximum.Should().Be(1500);
        episode.FundingType.Should().Be(FundingType.Levy);

        var price = request.OnProgramme.TotalPrice;
        episode.Prices.Count.Should().Be(1);
        var episodePrice = episode.Prices.Single();
        episodePrice.AgreedPrice.Should().Be(price);
        episodePrice.StartDate.Should().Be(request.OnProgramme.StartDate);
        episodePrice.EndDate.Should().Be(request.OnProgramme.ExpectedEndDate);
    }
}