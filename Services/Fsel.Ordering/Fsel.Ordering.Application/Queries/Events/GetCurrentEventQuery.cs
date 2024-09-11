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
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetCurrentEventQuery : IRequest<MethodResult<EventModel>>
    {
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

            var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);

            var @event = await _eventRepository.Queryable.Include(p => p.PackageEvents).FirstOrDefaultAsync(p => p.StartDate.HasValue && p.EndDate.HasValue && p.StartDate.Value <= currentDate && p.EndDate >= currentDate, cancellationToken);

            if (@event == null)
            {
                @event = await _eventRepository.Queryable.Include(p => p.PackageEvents).FirstOrDefaultAsync(p => p.IsDefault, cancellationToken);
            }
            methodResult.Result = _mapper.Map<EventModel>(@event);
            return methodResult;
        }
    }
}
