// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.GoogleSheetQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.IRepositories;
    using MediatR;

    public class GetListSchoolLuckySpinQuery : IRequest<MethodResult<IList<string?>>>
    {
    }

    public class GetListSchoolLuckySpinQueryHandler : IRequestHandler<GetListSchoolLuckySpinQuery, MethodResult<IList<string?>>>
    {
        private readonly ICompetitionEventsRepository _competitionEventsRepository;

        public GetListSchoolLuckySpinQueryHandler(ICompetitionEventsRepository competitionEventsRepository)
        {
            _competitionEventsRepository = competitionEventsRepository;
        }

        public async Task<MethodResult<IList<string?>>> Handle(GetListSchoolLuckySpinQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<string?>>();

            var schoolEvents = _competitionEventsRepository.Queryable
                             .AsEnumerable()
                             .Where(p => p.EventContent != null && p.EventContent.LuckySpin)
                             .ToList();


            schoolEvents = schoolEvents.Where(p => p.EventContent != null && p.EventContent.LuckySpin).ToList();

            var schoolCodes = schoolEvents.Select(p => p.EventCode).ToList();

            methodResult.Result = schoolCodes;
            return methodResult;
        }
    }
}
