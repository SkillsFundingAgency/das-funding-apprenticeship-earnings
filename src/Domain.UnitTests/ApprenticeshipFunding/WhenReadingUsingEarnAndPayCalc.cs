using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
using System;
using System.Linq;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.ApprenticeshipFunding;

[TestFixture]
public class WhenReadingUsingEarnAndPayCalc
{
    [Test]
    public void Then_A_New_Episode_Entity_Defaults_To_Using_The_Calculation()
    {
        new ApprenticeshipEpisodeEntity().UsingEarnAndPayCalc.Should().BeTrue();
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Then_The_Domain_Episode_Exposes_The_Entity_Value(bool usingEarnAndPayCalc)
    {
        var episodeEntity = CreateEpisode(usingEarnAndPayCalc);
        var learning = ApprenticeshipLearning.Get(new ApprenticeshipLearningEntity { Episodes = [episodeEntity] });

        learning.Episodes.Single().UsingEarnAndPayCalc.Should().Be(usingEarnAndPayCalc);
    }

    [Test]
    public void Then_The_Flag_Is_Held_Per_Episode()
    {
        var optedIn = CreateEpisode(true);
        var optedOut = CreateEpisode(false);
        var learning = ApprenticeshipLearning.Get(new ApprenticeshipLearningEntity { Episodes = [optedIn, optedOut] });

        learning.GetEpisode(optedIn.Key).UsingEarnAndPayCalc.Should().BeTrue();
        learning.GetEpisode(optedOut.Key).UsingEarnAndPayCalc.Should().BeFalse();
    }

    private static ApprenticeshipEpisodeEntity CreateEpisode(bool usingEarnAndPayCalc)
    {
        var key = Guid.NewGuid();
        return new ApprenticeshipEpisodeEntity
        {
            Key = key,
            UsingEarnAndPayCalc = usingEarnAndPayCalc,
            Prices = [new ApprenticeshipEpisodePriceEntity { Key = Guid.NewGuid(), EpisodeKey = key, StartDate = new DateTime(2026, 8, 1), EndDate = new DateTime(2027, 8, 1), AgreedPrice = 15000 }]
        };
    }
}
