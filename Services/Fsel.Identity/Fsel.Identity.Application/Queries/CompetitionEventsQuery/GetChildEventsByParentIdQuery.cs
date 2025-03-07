// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.CompetitionEventsQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetChildEventsByParentIdQuery : IRequest<MethodResult<IList<CompetitionEvent>?>>
    {
        public Guid Id { get; set; }
    }

    public class GetChildEventsByParentIdQueryHandler : IRequestHandler<GetChildEventsByParentIdQuery, MethodResult<IList<CompetitionEvent>?>>
    {
        private readonly ICompetitionEventsRepository _competitionEventRepository;
        private readonly IMapper _mapper;

        public GetChildEventsByParentIdQueryHandler(ICompetitionEventsRepository competitionEventRepository, IMapper mapper)
        {
            _competitionEventRepository = competitionEventRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<CompetitionEvent>?>> Handle(GetChildEventsByParentIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<CompetitionEvent>?>();

            var competitionEvents = _competitionEventRepository.Queryable;
            var parentEvent = await competitionEvents.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (parentEvent != null && parentEvent.EventContent != null && parentEvent.EventContent.IsParentEvent.HasValue && parentEvent.EventContent.IsParentEvent.Value)
            {
                var leafEvents = await GetLeafEventsRecursive(competitionEvents, parentEvent.Id);
                methodResult.Result = leafEvents.ToList();
                return methodResult;
            }
            return methodResult;
        }

        public async Task<ICollection<CompetitionEvent>> GetLeafEventsRecursive(IQueryable<CompetitionEvent> events, Guid? parentId = null)
        {
            var children = await events.Where(e => e.ParentEventId == parentId).ToListAsync();

            if (!children.Any())
            {
                return new List<CompetitionEvent>();
            }

            var leafEvents = new List<CompetitionEvent>();
            foreach (var child in children)
            {
                var subChildren = await GetLeafEventsRecursive(events, child.Id);
                if (subChildren.Count == 0)
                {
                    leafEvents.Add(child);
                }
                else
                {
                    leafEvents.AddRange(subChildren);
                }
            }
            return leafEvents;
        }
    }
}
