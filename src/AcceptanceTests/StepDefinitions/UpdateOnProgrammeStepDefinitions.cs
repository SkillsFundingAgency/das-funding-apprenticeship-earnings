using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Model;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateOnProgrammeCommand;
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

        await _testContext.TestInnerApi.Put($"/learning/{_scenarioContext.Get<LearningCreatedEvent>().LearningKey}/on-programme", updateOnProgrammeRequest);

        var apprenticeshipEntity = await GetApprenticeshipEntity();
        
        _scenarioContext.Set(apprenticeshipEntity);
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
                    model.PriceStartDate.SetValue(item.ToDateTime());
                    break;

                case nameof(UpdateOnProgrammeModel.PriceEndDate):
                    model.PriceEndDate.SetValue(item.ToDateTime());
                    break;

                case nameof(UpdateOnProgrammeModel.NewTrainingPrice):
                    model.NewTrainingPrice.SetValue(item.ToDecimalValue());
                    break;

                case nameof(UpdateOnProgrammeModel.NewAssessmentPrice):
                    model.NewAssessmentPrice.SetValue(item.ToDecimalValue());
                    break;
                
                case nameof(UpdateOnProgrammeModel.DateOfBirth):
                    model.DateOfBirth.SetValue(item.ToDateTime());
                    break;

                case nameof(UpdateOnProgrammeModel.PauseDate):
                    model.PauseDate.SetValue(item.ToNullableDateTime());
                    break;

                case nameof(UpdateOnProgrammeModel.BreaksInLearning):
                    model.BreaksInLearning.SetValue(item.ToList<BreakInLearningItem>());
                    break;

                case nameof(UpdateOnProgrammeModel.CompletionDate):
                    model.CompletionDate.SetValue(item.ToNullableDateTime());
                    break;

                case nameof(UpdateOnProgrammeModel.WithdrawalDate):
                    model.WithdrawalDate.SetValue(item.ToNullableDateTime());
                    break;

                case nameof(UpdateOnProgrammeModel.HasEHCP):
                    model.HasEHCP.SetValue(item.ToBool());
                    break;

                case nameof(UpdateOnProgrammeModel.IsCareLeaver):
                    model.IsCareLeaver.SetValue(item.ToBool());
                    break;

                case nameof(UpdateOnProgrammeModel.CareLeaverEmployerConsentGiven):
                    model.CareLeaverEmployerConsentGiven.SetValue(item.ToBool());
                    break;
            }
        }

        return model;
    }

    private async Task<ApprenticeshipModel> GetApprenticeshipEntity()
    {
        return await _testContext.SqlDatabase.GetApprenticeship(_scenarioContext.Get<LearningCreatedEvent>().LearningKey);
    }

}
