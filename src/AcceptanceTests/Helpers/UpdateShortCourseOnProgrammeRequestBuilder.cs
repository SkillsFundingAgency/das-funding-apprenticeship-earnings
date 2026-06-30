using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Model;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateShortCourseOnProgrammeCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Helpers;

public class UpdateShortCourseOnProgrammeRequestBuilder
{
    private DateTime? _withdrawalDate;
    private DateTime? _completionDate;
    private DateTime _startDate;
    private DateTime _expectedEndDate;
    private List<Milestone> _milestones = new List<Milestone>();

    internal UpdateShortCourseOnProgrammeRequestBuilder WithExistingData(CreateUnapprovedShortCourseLearningRequest createUnapprovedShortCourseLearningRequest)
    {
        _withdrawalDate = createUnapprovedShortCourseLearningRequest.OnProgramme.WithdrawalDate;
        _completionDate = createUnapprovedShortCourseLearningRequest.OnProgramme.CompletionDate;
        _startDate = createUnapprovedShortCourseLearningRequest.OnProgramme.StartDate;
        _expectedEndDate = createUnapprovedShortCourseLearningRequest.OnProgramme.ExpectedEndDate;
        _milestones = createUnapprovedShortCourseLearningRequest.OnProgramme.Milestones;
        return this;
    }

    internal UpdateShortCourseOnProgrammeRequestBuilder WithDataFromSetupModel(UpdateShortCourseOnProgrammeModel data)
    {
        if(data.WithdrawalDate.HasChanged) _withdrawalDate = data.WithdrawalDate.Value;
        if(data.CompletionDate.HasChanged) _completionDate = data.CompletionDate.Value;
        if(data.StartDate.HasChanged) _startDate = data.StartDate.Value!.Value;
        if(data.ExpectedEndDate.HasChanged) _expectedEndDate = data.ExpectedEndDate.Value!.Value;
        if(data.Milestones.HasChanged) _milestones = data.Milestones.Value;
        return this;
    }

    internal UpdateShortCourseOnProgrammeRequest Build(ScenarioContext scenarioContext)
    {
        return new UpdateShortCourseOnProgrammeRequest
        {
            WithdrawalDate = _withdrawalDate,
            CompletionDate = _completionDate,
            StartDate = _startDate,
            ExpectedEndDate = _expectedEndDate,
            Milestones = _milestones,
            LearnerKey = scenarioContext.GetLearnerKey(),
            LearnerRef = scenarioContext.GetLearnerRef()
        };
    }
}
