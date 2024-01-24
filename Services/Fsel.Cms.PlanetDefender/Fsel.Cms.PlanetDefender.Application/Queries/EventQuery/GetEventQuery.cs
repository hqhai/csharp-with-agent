// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Queries.NewsAndUpdateQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Cms.PlanetDefender.Domain.IRepositories;
    using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetEventQuery : IRequest<MethodResult<EventModel>>
    {
        public Guid Id { get; set; }
    }
    public class GetNewsAndUpdateQueryHandler : IRequestHandler<GetEventQuery, MethodResult<EventModel>>
    {
        private readonly IMapper _mapper;
        private readonly IEventRepository _newsAndUpdateRepository;

        public GetNewsAndUpdateQueryHandler(IMapper mapper, IEventRepository newsAndUpdateRepository)
        {
            _mapper = mapper;
            _newsAndUpdateRepository = newsAndUpdateRepository;
        }

        public async Task<MethodResult<EventModel>> Handle(GetEventQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<EventModel> methodResult = new MethodResult<EventModel>();

            var newsAndUpdate = await _newsAndUpdateRepository.GetByIdAsync(request.Id);

            if (newsAndUpdate == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(newsAndUpdate));
                return methodResult;
            }

            methodResult.Result = _mapper.Map<EventModel>(newsAndUpdate);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
