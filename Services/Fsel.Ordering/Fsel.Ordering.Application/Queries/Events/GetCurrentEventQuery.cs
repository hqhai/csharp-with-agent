// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.Events
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetCurrentEventQuery : IRequest<MethodResult<EventModel>>
    {
        public Guid? EventId { get; set; }
        public bool? IsDefault { get; set; }
    }

    public class GetCurrentEventQueryHandler : IRequestHandler<GetCurrentEventQuery, MethodResult<EventModel>>
    {
        private readonly IEventRepository _eventRepository;
        private readonly IMapper _mapper;

        public GetCurrentEventQueryHandler(IEventRepository eventRepository, IMapper mapper)
        {
            _eventRepository = eventRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<EventModel>> Handle(GetCurrentEventQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<EventModel>();

            var query = _eventRepository.Queryable.Include(p => p.PackageEvents)
                                                  .Where(x => x.Status == EnumEventPackageStatus.Active)
                                                  .AsNoTracking();
            if (request.IsDefault.HasValue && request.IsDefault.Value)
            {
                var defaultEvent = await query
                    .FirstOrDefaultAsync(p => p.IsDefault, cancellationToken);

                methodResult.Result = _mapper.Map<EventModel>(defaultEvent);
                return methodResult;
            }

            if (request.EventId.HasValue)
            {
                var defaultEvent = await query
                    .FirstOrDefaultAsync(p => p.Id == request.EventId, cancellationToken);

                methodResult.Result = _mapper.Map<EventModel>(defaultEvent);
                return methodResult;
            }

            var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);
            var @event = await query.Where(x => x.StartDate.HasValue && x.StartDate.Value <= currentDate)
                                    .FirstOrDefaultAsync(p => p.EndDate.HasValue && p.EndDate >= currentDate, cancellationToken);
            if (@event == null)
            {
                @event = await query.FirstOrDefaultAsync(p => p.IsDefault, cancellationToken);
            }
            methodResult.Result = _mapper.Map<EventModel>(@event);
            return methodResult;
        }
    }
}
