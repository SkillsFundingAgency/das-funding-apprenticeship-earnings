using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.TestHelpers;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using System;
using System.Linq;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.ApprenticeshipFunding;

[TestFixture]
internal class WhenAddingShortCourseEpisode
{
    private readonly Fixture _fixture = new();
    private ShortCourseLearning _learning;
    private Guid _existingEpisodeKey;

    [SetUp]
    public void SetUp()
    {
        _learning = _fixture.CreateLearningWithShortCourse(new DateTime(2024, 1, 1), new DateTime(2024, 3, 31), 3000m);
        _existingEpisodeKey = _learning.Episodes.Single().EpisodeKey;
    }

    [Test]
    public void HasEpisode_WhenEpisodeExists_ReturnsTrue()
    {
        _learning.HasEpisode(_existingEpisodeKey).Should().BeTrue();
    }

    [Test]
    public void HasEpisode_WhenEpisodeDoesNotExist_ReturnsFalse()
    {
        _learning.HasEpisode(Guid.NewGuid()).Should().BeFalse();
    }

    [Test]
    public void AddUnapprovedEpisode_AddsEpisodeToLearning()
    {
        var request = BuildRequest();

        _learning.AddUnapprovedEpisode(request);

        _learning.Episodes.Should().HaveCount(2);
    }

    [Test]
    public void AddUnapprovedEpisode_NewEpisodeHasCorrectKey()
    {
        var request = BuildRequest();

        _learning.AddUnapprovedEpisode(request);

        _learning.Episodes.Should().ContainSingle(e => e.EpisodeKey == request.EpisodeKey);
    }

    [Test]
    public void AddUnapprovedEpisode_OriginalEpisodeIsUnchanged()
    {
        var request = BuildRequest();

        _learning.AddUnapprovedEpisode(request);

        _learning.Episodes.Should().ContainSingle(e => e.EpisodeKey == _existingEpisodeKey);
    }

    private CreateUnapprovedShortCourseLearningRequest BuildRequest()
    {
        var request = _fixture.Create<CreateUnapprovedShortCourseLearningRequest>();
        request.LearningKey = _learning.LearningKey;
        return request;
    }
}
