﻿using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
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
            
            var response = new GetAcademicYearEarningsResponse { AcademicYearEarnings = academicYearEarnings };

            return await Task.FromResult(response);
        }
    }
}