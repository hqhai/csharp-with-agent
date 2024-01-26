// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Queries.EventQuery
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Cms.PlanetDefender.Domain.IRepositories;
    using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
    using Fsel.Common.ActionResults;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetListEventQuery : IRequest<MethodResult<IList<EventModel>>>
    {
    }

    public class GetListEventQueryHandler : IRequestHandler<GetListEventQuery, MethodResult<IList<EventModel>>>
    {
        private readonly IMapper _mapper;
        private readonly IEventRepository _eventRepository;

        public GetListEventQueryHandler(IMapper mapper, IEventRepository eventRepository)
        {
            _mapper = mapper;
            _eventRepository = eventRepository;
        }

        public async Task<MethodResult<IList<EventModel>>> Handle(GetListEventQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<EventModel>>();

            var events = await _eventRepository.Queryable
                                               .Where(m => m.StartDate.Date == DateTime.UtcNow.Date &&
                                                           m.StartDate.Month == DateTime.UtcNow.Month &&
                                                           m.StartDate.Year == DateTime.UtcNow.Year)
                                               .ToListAsync(cancellationToken);

            methodResult.Result = _mapper.Map<IList<EventModel>>(events);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
