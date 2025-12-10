using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Model;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.TestHelpers;
using SFA.DAS.Learning.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow.Assist;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.StepDefinitions;

[Binding]
public class UpdateOnProgrammeStepDefinitions
{
    private readonly ScenarioContext _scenarioContext;
    private readonly TestContext _testContext;

    public UpdateOnProgrammeStepDefinitions(ScenarioContext scenarioContext, TestContext testContext)
    {
        _scenarioContext = scenarioContext;
        _testContext = testContext;
    }

    [Given(@"the following on-programme request is sent")]
    [When("the following on-programme request is sent")]
    public async Task SendOnProgrammeRequest(Table table)
    {
        var data = GetUpdateOnProgrammeModel(table);

        var updateOnProgrammeRequest = _scenarioContext.GetUpdateOnProgrammeRequestBuilder()
            .WithExistingApprenticeshipData(_scenarioContext.Get<LearningCreatedEvent>())
            .WithDataFromSetupModel(data)
            .Build(_testContext.FundingBandMaximumService.GetFundingBandMaximum());

        await _testContext.TestInnerApi.Put($"/apprenticeship/{_scenarioContext.Get<LearningCreatedEvent>().LearningKey}/on-programme", updateOnProgrammeRequest);

        await WaitHelper.WaitForItAsync(async () => await EnsureRecalculationHasHappened(), "Failed to publish earning recalculation");

        _scenarioContext.Set(updateOnProgrammeRequest);
    }

    private UpdateOnProgrammeModel GetUpdateOnProgrammeModel(Table table)
    {
        var data = table.CreateSet<KeyValueModel>().ToList();
        var model = new UpdateOnProgrammeModel();

        foreach (var item in data)
        {
            switch (item.Key)
            {
                case nameof(UpdateOnProgrammeModel.PriceStartDate):
                    model.PriceStartDate = item.ToDateTime();
                    break;

                case nameof(UpdateOnProgrammeModel.PriceEndDate):
                    model.PriceEndDate = item.ToDateTime();
                    break;

                case nameof(UpdateOnProgrammeModel.NewTrainingPrice):
                    model.NewTrainingPrice = item.ToDecimalValue();
                    break;

                case nameof(UpdateOnProgrammeModel.NewAssessmentPrice):
                    model.NewAssessmentPrice = item.ToDecimalValue();
                    break;
                

            }
        }

        return model;
    }

    private async Task<ApprenticeshipModel> GetApprenticeshipEntity()
    {
        return await _testContext.SqlDatabase.GetApprenticeship(_scenarioContext.Get<LearningCreatedEvent>().LearningKey);
    }

    private async Task<bool> EnsureRecalculationHasHappened()
    {
        var apprenticeshipEntity = await GetApprenticeshipEntity();
        var currentEpisode = apprenticeshipEntity!.GetCurrentEpisode(TestSystemClock.Instance());

        var history = await _testContext.SqlDatabase.GetHistory(currentEpisode.EarningsProfile.EarningsProfileId);

        if (!history.Any())
        {
            return false;
        }

        //History always contains 1 record for the initial creation
        //Therefore, we must disregard this when looking for recalculation
        if (history.Count == 1)
        {
            return false;
        }

        _scenarioContext.Set(apprenticeshipEntity);
        return true;
    }
}
