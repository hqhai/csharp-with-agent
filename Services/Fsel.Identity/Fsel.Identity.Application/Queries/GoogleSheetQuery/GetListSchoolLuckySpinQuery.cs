// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.GoogleSheetQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.IRepositories;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetListSchoolLuckySpinQuery : IRequest<MethodResult<IList<string>?>>
    {
    }

    public class GetListSchoolLuckySpinQueryHandler : IRequestHandler<GetListSchoolLuckySpinQuery, MethodResult<IList<string>?>>
    {
        private readonly ICompetitionEventsRepository _competitionEventsRepository;

        public GetListSchoolLuckySpinQueryHandler(ICompetitionEventsRepository competitionEventsRepository)
        {
            _competitionEventsRepository = competitionEventsRepository;
        }

        public async Task<MethodResult<IList<string>?>> Handle(GetListSchoolLuckySpinQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<string>?>();

            var schoolEvents = await _competitionEventsRepository.Queryable.Where(p => p.EventContent != null && p.EventContent.Count > 0).SelectMany(p => p.EventContent!).ToListAsync(cancellationToken);

            schoolEvents = schoolEvents.Where(p => p.LuckySpin).DistinctBy(p => p.SchoolCode).ToList();

            var schoolCodes = schoolEvents.Where(p => !string.IsNullOrEmpty(p.SchoolCode)).Select(p => p.SchoolCode ?? string.Empty).ToList();

            methodResult.Result = schoolCodes;
            return methodResult;
        }
    }
}
