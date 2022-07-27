using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.Apprenticeships.Events;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests;

public class AdjustedPriceProcessor_CalculateAdjustedPriceTests
{
    private AdjustedPriceProcessor _sut;

    [TestCase(15000, 12000)]
    [TestCase(18777, 15021.60)]
    public void ShouldReturn80PercentOfAgreedPrice(decimal agreedPrice, decimal expectedAdjustedPrice)
    {
        _sut = new AdjustedPriceProcessor();

        var actualAdjustedPrice = _sut.CalculateAdjustedPrice(agreedPrice);

        actualAdjustedPrice.Should().Be(expectedAdjustedPrice);
    }
}

public class EarningsProfileGenerator_GenerateEarningsTests
{
    private EarningsProfileGenerator _sut;
    private ApprenticeshipCreatedEvent _apprenticeshipLearnerEvent;
    private Mock<IAdjustedPriceProcessor> _mockAdjustedPriceProcessor;
    private Mock<IInstallmentsGenerator> _mockInstallmentsGenerator;
    private Mock<IMessageSession> _mockMessageSession;
    private Mock<IEarningsGeneratedEventBuilder> _mockEarningsGeneratedEventBuilder;
    private decimal _expectedAdjustedPrice;
    private List<EarningsInstallment> _expectedEarningsInstallments;
    private EarningsGeneratedEvent _expectedEarningsGeneratedEvent;
    private EarningsProfile _result;

    [SetUp]
    public async Task SetUp()
    {
        _apprenticeshipLearnerEvent = new ApprenticeshipCreatedEvent
        {
            FundingType = FundingType.NonLevy,
            ActualStartDate = new DateTime(2022, 8, 1),
            ApprenticeshipKey = Guid.NewGuid(),
            EmployerAccountId = 114,
            PlannedEndDate = new DateTime(2024, 7, 31),
            UKPRN = 116,
            TrainingCode = "able-seafarer",
            FundingEmployerAccountId = 118,
            Uln = 900000118,
            AgreedPrice = 15000,
            ApprovalsApprenticeshipId = 120,
            LegalEntityName = "MyTrawler"
        };

        _expectedAdjustedPrice = 12000;

        _mockAdjustedPriceProcessor = new Mock<IAdjustedPriceProcessor>();
        _mockAdjustedPriceProcessor.Setup(x => x.CalculateAdjustedPrice(It.IsAny<decimal>()))
            .Returns(_expectedAdjustedPrice);

        _expectedEarningsInstallments = new List<EarningsInstallment>
            {
                new EarningsInstallment
                {
                    Amount = 1000,
                    AcademicYear = 1920,
                    DeliveryPeriod = 4
                },
                new EarningsInstallment
                {
                    Amount = 1000,
                    AcademicYear = 1920,
                    DeliveryPeriod = 5
                }
            };

        _mockInstallmentsGenerator = new Mock<IInstallmentsGenerator>();
        _mockInstallmentsGenerator
            .Setup(x => x.Generate(It.IsAny<decimal>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(_expectedEarningsInstallments);

        _mockMessageSession = new Mock<IMessageSession>();

        _expectedEarningsGeneratedEvent = new EarningsGeneratedEvent { ApprenticeshipKey = Guid.NewGuid() };

        _mockEarningsGeneratedEventBuilder = new Mock<IEarningsGeneratedEventBuilder>();

        _mockEarningsGeneratedEventBuilder.Setup(x => x.Build(It.IsAny<ApprenticeshipCreatedEvent>(), It.IsAny<EarningsProfile>())).Returns(_expectedEarningsGeneratedEvent);

        _sut = new EarningsProfileGenerator(_mockAdjustedPriceProcessor.Object, _mockInstallmentsGenerator.Object, _mockMessageSession.Object, _mockEarningsGeneratedEventBuilder.Object);
        _result = await _sut.GenerateEarnings(_apprenticeshipLearnerEvent);
    }

    [Test]
    public void ShouldPassTheAgreedPriceToTheAdjustedPriceProcessor()
    {
        _mockAdjustedPriceProcessor.Verify(x => x.CalculateAdjustedPrice(_apprenticeshipLearnerEvent.AgreedPrice));
    }

    [Test]
    public void ShouldSetTheAdjustedPriceToTheResultFromTheAdjustedPriceProcessor()
    {
        _result.AdjustedPrice.Should().Be(_expectedAdjustedPrice);
    }

    [Test]
    public void ShouldPassTheAdjustedPriceAndCorrectDatesToTheInstallmentsGenerator()
    {
        _mockInstallmentsGenerator.Verify(x => x.Generate(_expectedAdjustedPrice, _apprenticeshipLearnerEvent.ActualStartDate.GetValueOrDefault(), _apprenticeshipLearnerEvent.PlannedEndDate.GetValueOrDefault()));
    }

    [Test]
    public void ShouldSetTheInstallmentsToTheResultFromTheInstallmentsGenerator()
    {
        _result.Installments.Should().BeEquivalentTo(_expectedEarningsInstallments);
    }

    [Test]
    public void ShouldPublishEarningsGeneratedEventCorrectly()
    {
        _mockMessageSession.Verify(x => x.Publish(_expectedEarningsGeneratedEvent, It.IsAny<PublishOptions>()));
    }
}