using SFA.DAS.Learning.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataTransferObjects;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Queries;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Queries.GetAcademicYearEarnings
{
    public class GetAcademicYearEarningsQueryHandler : IQueryHandler<GetAcademicYearEarningsRequest, GetAcademicYearEarningsResponse>
    {
        private readonly IEarningsQueryRepository _earningsQueryRepository;
        private readonly IAcademicYearService _academicYearService;

        public GetAcademicYearEarningsQueryHandler(IEarningsQueryRepository earningsQueryRepository, IAcademicYearService academicYearService)
        {
            _earningsQueryRepository = earningsQueryRepository;
            _academicYearService = academicYearService;
        }

        public async Task<GetAcademicYearEarningsResponse> Handle(GetAcademicYearEarningsRequest query, CancellationToken cancellationToken = default)
        {
            var academicYearEarnings = await _earningsQueryRepository.GetAcademicYearEarnings(query.Ukprn, _academicYearService.CurrentAcademicYear);

            academicYearEarnings.Learners.ForEach(l => CalculateCoInvestmentAmounts(l));

            var response = new GetAcademicYearEarningsResponse(academicYearEarnings);

            return await Task.FromResult(response);
        }

        private void CalculateCoInvestmentAmounts(Learner learner)
        {
            if (learner.FundingType == FundingType.NonLevy)
            {
                learner.OnProgrammeEarnings.ForEach(e => CalculateCoInvestmentForEarning(learner.IsNoneLevyFullyFunded, e));
            }
        }

        private void CalculateCoInvestmentForEarning(bool isNoneLevyFullyFunded, OnProgrammeEarning onProgrammeEarning)
        {
            var coinvestment = CoInvestment.Calculate(isNoneLevyFullyFunded, onProgrammeEarning.Amount);
            onProgrammeEarning.GovernmentContribution = coinvestment.GovernmentContribution;
            onProgrammeEarning.EmployerContribution = coinvestment.EmployerContribution;
        }
    }
}
