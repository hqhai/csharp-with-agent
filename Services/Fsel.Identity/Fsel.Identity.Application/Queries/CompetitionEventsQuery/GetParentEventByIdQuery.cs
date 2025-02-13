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

    public class GetParentEventByIdQuery : IRequest<MethodResult<Guid?>>
    {
        public Guid Id { get; set; }
    }

    public class GetParentEventByIdQueryHandler : IRequestHandler<GetParentEventByIdQuery, MethodResult<Guid?>>
    {
        private readonly ICompetitionEventsRepository _competitionEventRepository;
        private readonly IMapper _mapper;

        public GetParentEventByIdQueryHandler(ICompetitionEventsRepository competitionEventRepository, IMapper mapper)
        {
            _competitionEventRepository = competitionEventRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<Guid?>> Handle(GetParentEventByIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Guid?>();

            var competitionEvents = await _competitionEventRepository.Queryable.ToListAsync(cancellationToken);
            var competitionChildEvent = competitionEvents.FirstOrDefault(x => x.Id == request.Id);

            if (competitionChildEvent != null)
            {
                var parentId = GetRootIdRecursive(competitionEvents, competitionChildEvent.Id);
                var competitionParentEvent = competitionEvents.FirstOrDefault(x => x.Id == parentId);

                methodResult.Result = competitionParentEvent?.Id ?? Guid.Empty;
                return methodResult;
            }

            return methodResult;
        }

        public Guid GetRootIdRecursive(IEnumerable<CompetitionEvent> records, Guid currentId)
        {
            var currentRecord = records.FirstOrDefault(r => r.Id == currentId);
            if (currentRecord == null || currentRecord.ParentEventId == null)
            {
                return currentId; // Đây là bảng gốc
            }
            return GetRootIdRecursive(records, currentRecord.ParentEventId.Value);
        }

    }
}
