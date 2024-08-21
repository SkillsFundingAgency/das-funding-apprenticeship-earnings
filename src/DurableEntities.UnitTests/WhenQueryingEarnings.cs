using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.ContextImplementations;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.Options;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.CreateApprenticeshipCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.ProcessUpdatedEpisodeCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GetApprenticeshipByUkprnResponse = SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models.GetApprenticeshipByUkprnResponse.Apprenticeship;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.UnitTests;

[TestFixture]
public class WhenQueryingEarnings
{
    private Fixture _fixture;
    private Mock<IDurableClientFactory> _mockClientFactory;
    private Mock<IDurableClient> _mockDurableClient;
    private Mock<IOptions<DurableTaskOptions>> _mockOptions;
    private Mock<ISystemClockService> _mockSystemClockService;
    private Mock<ICreateApprenticeshipCommandHandler> _mockCreateApprenticeshipCommandHandler;
    private Mock<IDomainEventDispatcher> _mockDomainEventDispatcher;
    private Mock<IProcessEpisodeUpdatedCommandHandler> _mockProcessEpisodeUpdatedCommandHandler;
    private QueryFunction _queryFunction;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _mockClientFactory = new Mock<IDurableClientFactory>();
        _mockDurableClient = new Mock<IDurableClient>();
        _mockOptions = new Mock<IOptions<DurableTaskOptions>>();
        _mockSystemClockService = new Mock<ISystemClockService>();
        _mockSystemClockService.Setup(s => s.UtcNow).Returns(DateTime.UtcNow);

        _mockOptions.Setup(o => o.Value).Returns(new DurableTaskOptions { HubName = "TestHub" });
        _mockClientFactory.Setup(f => f.CreateClient(It.IsAny<DurableClientOptions>())).Returns(_mockDurableClient.Object);

        _mockCreateApprenticeshipCommandHandler = new Mock<ICreateApprenticeshipCommandHandler>();
        _mockDomainEventDispatcher = new Mock<IDomainEventDispatcher>();
        _mockProcessEpisodeUpdatedCommandHandler = new Mock<IProcessEpisodeUpdatedCommandHandler>();

        _queryFunction = new QueryFunction(_mockClientFactory.Object, _mockOptions.Object, _mockSystemClockService.Object);
    }

    [Test]
    public async Task GetApprenticeshipEntites_ReturnsOkResult_WithEntities()
    {
        // Arrange
        var httpRequest = new Mock<HttpRequest>();
        var ukprn = _fixture.Create<long>();
        var queryResult = CreateEntityQueryResult(
            new List<ApprenticeshipEntity>
            {
                CreateApprenticeshipEntity(ukprn)
            }
        );

        _mockDurableClient.Setup(c => c.ListEntitiesAsync(It.IsAny<EntityQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(queryResult);

        // Act
        var result = await _queryFunction.GetApprenticeshipEntites(httpRequest.Object, ukprn);

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        var entities = okResult.Value as List<GetApprenticeshipByUkprnResponse>;
        entities.Should().NotBeNull();
        entities.Should().ContainSingle();
    }

    [Test]
    public async Task GetApprenticeshipEntites_ReturnsOkResult_WithNoEntities()
    {
        // Arrange
        var httpRequest = new Mock<HttpRequest>();
        var ukprn = 12345678L;
        var queryResult = CreateEntityQueryResult(new List<ApprenticeshipEntity>());

        _mockDurableClient.Setup(c => c.ListEntitiesAsync(It.IsAny<EntityQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(queryResult);

        // Act
        var result = await _queryFunction.GetApprenticeshipEntites(httpRequest.Object, ukprn);

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        var entities = okResult.Value as List<GetApprenticeshipByUkprnResponse>;
        entities.Should().NotBeNull();
        entities.Should().BeEmpty();
    }

    private EntityQueryResult CreateEntityQueryResult(List<ApprenticeshipEntity> apprenticeshipEntities)
    {

        var entityQueryResultType = CreateInstance<EntityQueryResult>();

        var entities = new List<DurableEntityStatus>();

        foreach (var apprenticeship in apprenticeshipEntities)
        {
            var entity = CreateInstance<DurableEntityStatus>();

            entity.EntityId = new EntityId(apprenticeship.Model.ApprenticeshipKey.ToString(), "Apprenticeship");
            entity.State = JToken.FromObject(apprenticeship);
            entities.Add(entity);
        }

        entityQueryResultType.Entities = entities.ToArray();

        return entityQueryResultType;

    }

    private T CreateInstance<T>()
    {
        var entityQueryResultType = typeof(T);
        var instance = (T)Activator.CreateInstance(
            entityQueryResultType,
            BindingFlags.Instance | BindingFlags.NonPublic,
            null,
            null,
            null
        );

        return instance;
    }

    private ApprenticeshipEntity CreateApprenticeshipEntity(long ukprn)
    {
        var apprenticeshipEntity = new ApprenticeshipEntity(_mockCreateApprenticeshipCommandHandler.Object, _mockDomainEventDispatcher.Object, _mockProcessEpisodeUpdatedCommandHandler.Object);
        
        var model = _fixture.Create<ApprenticeshipEntityModel>();
        var episode = _fixture.Create<ApprenticeshipEpisodeModel>();
        var price = _fixture.Create<PriceModel>();

        price.ActualStartDate = DateTime.UtcNow.AddMonths(-3);
        price.PlannedEndDate = DateTime.UtcNow.AddMonths(3);

        episode.UKPRN = ukprn;
        episode.Prices = new List<PriceModel> { price };


        model.ApprenticeshipEpisodes = new List<ApprenticeshipEpisodeModel> { episode };
        

        apprenticeshipEntity.Model = model;

        return apprenticeshipEntity;
    }
}
