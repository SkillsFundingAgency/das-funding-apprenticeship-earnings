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
    private List<Milestone> _milestones = new List<Milestone>();

    internal UpdateShortCourseOnProgrammeRequestBuilder WithExistingData(CreateUnapprovedShortCourseLearningRequest createUnapprovedShortCourseLearningRequest)
    {
        _withdrawalDate = createUnapprovedShortCourseLearningRequest.OnProgramme.WithdrawalDate;
        _milestones = createUnapprovedShortCourseLearningRequest.OnProgramme.Milestones;
        return this;
    }

    internal UpdateShortCourseOnProgrammeRequestBuilder WithDataFromSetupModel(UpdateShortCourseOnProgrammeModel data)
    {
        if(data.WithdrawalDate.HasChanged) _withdrawalDate = data.WithdrawalDate.Value;
        if(data.Milestones.HasChanged) _milestones = data.Milestones.Value;
        return this;
    }

    internal UpdateShortCourseOnProgrammeRequest Build()
    {
        return new UpdateShortCourseOnProgrammeRequest
        {
            WithdrawalDate = _withdrawalDate,
            Milestones = _milestones
        };
    }
}
