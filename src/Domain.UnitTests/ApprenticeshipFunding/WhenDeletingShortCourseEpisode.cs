using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.TestHelpers;
using System;
using System.Linq;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.ApprenticeshipFunding;

[TestFixture]
internal class WhenDeletingShortCourseEpisode
{
    private readonly Fixture _fixture = new();
    private ShortCourseLearning _learning;
    private ShortCourseEpisode _episode;

    [SetUp]
    public void SetUp()
    {
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 3, 31);

        _learning = _fixture.CreateLearningWithShortCourse(startDate, endDate, 3000m);
        _episode = _learning.Episodes.Single();
        _episode.UpdateWithdrawalDate(null);
        _episode.CalculateShortCourseOnProgram("initial");
    }

    [Test]
    public void ThenWithdrawalDateIsSetToStartDate()
    {
        _episode.Delete();

        _episode.WithdrawalDate.Should().Be(_episode.StartDate);
    }

    [Test]
    public void ThenCompletionDateIsCleared()
    {
        _episode.UpdateCompletion(new DateTime(2024, 3, 1));

        _episode.Delete();

        _episode.CompletionDate.Should().BeNull();
    }

    [Test]
    public void ThenInstalmentsAreRemoved()
    {
        _episode.Delete();

        _episode.EarningsProfile.Instalments.Should().BeEmpty();
    }
}
