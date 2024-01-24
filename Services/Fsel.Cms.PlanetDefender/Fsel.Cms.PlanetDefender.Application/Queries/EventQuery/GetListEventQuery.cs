// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Queries.NewsAndUpdateQuery
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

    public class GetListNewsAndUpdateQueryHandler : IRequestHandler<GetListEventQuery, MethodResult<IList<EventModel>>>
    {
        private readonly IMapper _mapper;
        private readonly IEventRepository _newsAndUpdateRepository;

        public GetListNewsAndUpdateQueryHandler(IMapper mapper, IEventRepository newsAndUpdateRepository)
        {
            _mapper = mapper;
            _newsAndUpdateRepository = newsAndUpdateRepository;
        }

        public async Task<MethodResult<IList<EventModel>>> Handle(GetListEventQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<EventModel>> methodResult = new MethodResult<IList<EventModel>>();

            var newsAndUpdate = await _newsAndUpdateRepository.Queryable.Where(m => m.StartDate >= DateTime.UtcNow && DateTime.UtcNow <= DateTime.UtcNow.AddDays(1)).ToListAsync(cancellationToken);

            methodResult.Result = _mapper.Map<IList<EventModel>>(newsAndUpdate);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
