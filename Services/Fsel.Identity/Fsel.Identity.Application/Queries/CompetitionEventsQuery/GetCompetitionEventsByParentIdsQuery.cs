// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.CompetitionEventsQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetCompetitionEventsByParentIdsQuery : IRequest<MethodResult<IList<CompetitionEventsModel>>>
    {
        public IList<Guid>? ParentIds { get; set; }
    }

    public class GetCompetitionEventsByParentIdsQueryHandler : IRequestHandler<GetCompetitionEventsByParentIdsQuery, MethodResult<IList<CompetitionEventsModel>>>
    {
        private readonly ICompetitionEventsRepository _competitionEventRepository;
        private readonly IMapper _mapper;

        public GetCompetitionEventsByParentIdsQueryHandler(ICompetitionEventsRepository competitionEventRepository, IMapper mapper)
        {
            _competitionEventRepository = competitionEventRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<CompetitionEventsModel>>> Handle(GetCompetitionEventsByParentIdsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<CompetitionEventsModel>>();

            var competitionEvents = await _competitionEventRepository.Queryable.Where(p => request.ParentIds != null && p.ParentEventId.HasValue && request.ParentIds.Contains(p.ParentEventId.Value)).ToListAsync(cancellationToken);

            methodResult.Result = _mapper.Map<IList<CompetitionEventsModel>>(competitionEvents);
            return methodResult;
        }
    }
}
