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
    private Domain.Factories.LearningFactory _factory;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _factory = new Domain.Factories.LearningFactory();
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
                TotalPrice = 1500m,
                Ukprn = 10005555
            }
        };

        // Act
        var learning = _factory.CreateNewShortCourse(request);

        // Assert
        learning.LearningKey.Should().Be(request.LearningKey);
        learning.DateOfBirth.Should().Be(request.Learner.DateOfBirth);
        learning.Uln.Should().Be(request.Learner.Uln);

        var episode = learning.Episodes.SingleOrDefault();
        episode.Should().NotBeNull();
        episode.EmployerAccountId.Should().Be(request.OnProgramme.EmployerId);
        episode.TrainingCode.Should().Be(request.OnProgramme.CourseCode);
        episode.AgeAtStartOfApprenticeship.Should().Be(25); // 2025 - 2000
        episode.FundingType.Should().Be(FundingType.Levy);
        episode.UKPRN.Should().Be(request.OnProgramme.Ukprn);

        var price = request.OnProgramme.TotalPrice;
        episode.CoursePrice.Should().Be(price);
        episode.StartDate.Should().Be(request.OnProgramme.StartDate);
        episode.EndDate.Should().Be(request.OnProgramme.ExpectedEndDate);
    }
}