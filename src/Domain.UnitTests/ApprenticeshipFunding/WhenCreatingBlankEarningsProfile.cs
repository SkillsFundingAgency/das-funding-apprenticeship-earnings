using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.EnglishAndMaths;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.TestHelpers;
using SFA.DAS.Funding.ApprenticeshipEarnings.TestHelpers;
using ApprenticeshipEarningsRecalculatedEvent = SFA.DAS.Funding.ApprenticeshipEarnings.Types.ApprenticeshipEarningsRecalculatedEvent;
using EarningsProfileUpdatedEvent = SFA.DAS.Funding.ApprenticeshipEarnings.Types.EarningsProfileUpdatedEvent;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.ApprenticeshipFunding;

[TestFixture]
public class WhenCreatingBlankEarningsProfile
{
    private Fixture _fixture = null!;
    private Mock<ISystemClockService> _mockSystemClockService = null!;
    private readonly DateTime _startDate = new(2021, 1, 15);
    private readonly DateTime _plannedEndDate = new(2022, 1, 31);

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _mockSystemClockService = new Mock<ISystemClockService>();
        _mockSystemClockService.Setup(x => x.UtcNow).Returns(new DateTime(2021, 8, 30));
    }

    [Test]
    public void Then_A_Profile_With_No_Earnings_Is_Created()
    {
        var (learning, episode) = CreateLearningWithoutProfile();

        episode.CreateBlankEarningsProfile(isApproved: false);

        var profile = episode.EarningsProfile!;
        profile.Instalments.Should().BeEmpty();
        profile.AdditionalPayments.Should().BeEmpty();
        profile.MathsAndEnglishCourses.Should().BeEmpty();
        profile.OnProgramTotal.Should().Be(0);
        profile.CompletionPayment.Should().Be(0);
        profile.CalculationData.Should().Be("{}");
        ((ApprenticeshipLearningEntity)learning.GetModel()).Episodes.Single().EarningsProfile.Should().NotBeNull();
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Then_The_Approval_Status_Is_Set_As_Supplied(bool isApproved)
    {
        var (_, episode) = CreateLearningWithoutProfile();

        episode.CreateBlankEarningsProfile(isApproved);

        episode.EarningsProfile!.IsApproved.Should().Be(isApproved);
    }

    [Test]
    public void Then_The_Episode_Is_Marked_As_Not_Using_The_Calculation()
    {
        var (_, episode) = CreateLearningWithoutProfile();
        episode.UsingEarnAndPayCalc.Should().BeTrue();

        episode.CreateBlankEarningsProfile(isApproved: false);

        episode.UsingEarnAndPayCalc.Should().BeFalse();
    }

    [Test]
    public void Then_No_Recalculated_Event_Is_Raised()
    {
        var (learning, episode) = CreateLearningWithoutProfile();
        learning.FlushEvents();

        episode.CreateBlankEarningsProfile(isApproved: false);

        var events = learning.FlushEvents().ToList();
        events.OfType<ApprenticeshipEarningsRecalculatedEvent>().Should().BeEmpty();
        events.OfType<EarningsProfileUpdatedEvent>().Should().ContainSingle().Which.InitialGeneration.Should().BeTrue();
    }

    [Test]
    public void Then_Existing_Profile_Cannot_Be_Replaced()
    {
        var (_, episode) = CreateLearningWithoutProfile();
        episode.CreateBlankEarningsProfile(isApproved: false);

        var act = () => episode.CreateBlankEarningsProfile(isApproved: false);

        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void Then_English_And_Maths_Can_Be_Stored_Without_Earnings()
    {
        var (learning, episode) = CreateLearningWithoutProfile();
        episode.CreateBlankEarningsProfile(isApproved: false);

        var courseStart = new DateTime(2021, 2, 1);
        var courseEnd = new DateTime(2021, 4, 30);
        var learningSupport = LearningSupportPayments
            .GenerateLearningSupportPaymentsWithoutEarnings(courseStart, courseEnd)
            .Select(p => new EnglishAndMathsAdditionalPayment(p.AcademicYear, p.DeliveryPeriod, p.Amount, p.DueDate, p.AdditionalPaymentType))
            .ToList();
        var course = EnglishAndMaths.CreateWithoutEarnings(courseStart, courseEnd, "Maths", "M102", 300, null, null, null, null,
            [PeriodInLearningHelper.Create(courseStart, courseEnd, courseEnd)], learningSupport);

        learning.UpdateEnglishAndMathsCourses([course], _mockSystemClockService.Object);

        var stored = episode.EarningsProfile!.MathsAndEnglishCourses.Should().ContainSingle().Subject;
        stored.Instalments.Should().BeEmpty();
        stored.AdditionalPayments.Should().OnlyContain(x => x.Amount == 0m);
        episode.EarningsProfile.Instalments.Should().BeEmpty();
    }

    [Test]
    public void Then_Calculating_Afterwards_Generates_Earnings_On_The_Same_Profile()
    {
        var (learning, episode) = CreateLearningWithoutProfile();
        episode.CreateBlankEarningsProfile(isApproved: false);
        var profileKey = episode.EarningsProfile!.EarningsProfileId;

        learning.Calculate(_mockSystemClockService.Object, "{}", episode.EpisodeKey, initialGenerationIsApproved: false);

        episode.EarningsProfile!.EarningsProfileId.Should().Be(profileKey);
        episode.EarningsProfile.Instalments.Should().NotBeEmpty();
        episode.EarningsProfile.OnProgramTotal.Should().BeGreaterThan(0);
    }

    private (ApprenticeshipLearning learning, ApprenticeshipEpisode episode) CreateLearningWithoutProfile()
    {
        var learning = _fixture.CreateLearningWithApprenticeship(_startDate, _plannedEndDate, 15000m);
        var episode = learning.Episodes.First();
        episode.EarningsProfile.Should().BeNull();
        return (learning, episode);
    }
}
