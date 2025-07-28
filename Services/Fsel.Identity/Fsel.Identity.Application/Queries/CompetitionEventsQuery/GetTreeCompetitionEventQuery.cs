// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.CompetitionEventsQuery
{
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetTreeCompetitionEventQuery : IRequest<MethodResult<IList<CompetitionEventTreeModel>>>
    {
        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public IList<Guid>? LocationIds { get; set; }
    }

    public class GetTreeCompetitionEventQueryHandler : IRequestHandler<GetTreeCompetitionEventQuery, MethodResult<IList<CompetitionEventTreeModel>>>
    {
        private readonly ICompetitionEventsRepository _competitionEventsRepository;
        private readonly IMapper _mapper;

        public GetTreeCompetitionEventQueryHandler(ICompetitionEventsRepository competitionEventsRepository,
                                                   IMapper mapper)
        {
            _competitionEventsRepository = competitionEventsRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<CompetitionEventTreeModel>>> Handle(GetTreeCompetitionEventQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<CompetitionEventTreeModel>> methodResult = new MethodResult<IList<CompetitionEventTreeModel>>();

            var eventParents = await _competitionEventsRepository.Queryable.Where(x => !x.ParentEventId.HasValue).ToListAsync(cancellationToken);

            if (request.StartDate.HasValue && request.EndDate.HasValue)
            {
                eventParents = eventParents.Where(x => request.StartDate <= x.EventContent?.EndDate && request.EndDate >= x.EventContent?.StartDate).ToList();
            }

            if (request.LocationIds != null)
            {
                eventParents = eventParents.Where(x => x.LocationId.HasValue && request.LocationIds.Contains(x.LocationId.Value)).ToList();
            }

            var eventParentResults = _mapper.Map<IList<CompetitionEventTreeModel>>(eventParents);

            if (eventParents != null)
            {
                await AddChildentEvent(eventParentResults, cancellationToken);
            }

            methodResult.Result = eventParentResults;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task AddChildentEvent(IList<CompetitionEventTreeModel> parentCompetitionEvents, CancellationToken cancellationToken)
        {
            var parentEventIds = parentCompetitionEvents.Select(x => x.Id).ToList();
            var childentEvents = await _competitionEventsRepository.Queryable
                                                                   .WhereBulkContains(parentEventIds, x => x.ParentEventId)
                                                                   .ToListAsync(cancellationToken);

            if (childentEvents == null || !childentEvents.Any())
            {
                return;
            }

            ConcurrentStack<CompetitionEventTreeModel> competitionEvents = new ConcurrentStack<CompetitionEventTreeModel>();

            Parallel.ForEach(parentCompetitionEvents, parentCompetitionEvent =>
            {
                var childentWithEventParents = childentEvents.Where(x => x.ParentEventId == parentCompetitionEvent.Id).ToList();
                var parentCompetitionEventChildents = _mapper.Map<IList<CompetitionEventTreeModel>>(childentWithEventParents);
                parentCompetitionEvent.Childents = parentCompetitionEventChildents;

                competitionEvents.PushRange(parentCompetitionEventChildents.ToArray());
            });

            await AddChildentEvent(competitionEvents.ToArray(), cancellationToken);
        }
    }
}
