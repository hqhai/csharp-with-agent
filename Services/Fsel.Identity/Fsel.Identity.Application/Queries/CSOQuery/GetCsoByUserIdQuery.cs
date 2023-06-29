// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.CsoQuery
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetCsoByUserIdQuery : IRequest<MethodResult<CSOModel>>
    {
        public Guid UserId { get; set; }
    }

    public class GetCsoByUserIdQueryHandler : IRequestHandler<GetCsoByUserIdQuery, MethodResult<CSOModel>>
    {
        private readonly IMapper _mapper;
        private readonly ICSORepository _csoRepository;

        public GetCsoByUserIdQueryHandler(IMapper mapper, ICSORepository csoRepository)
        {
            _mapper = mapper;
            _csoRepository = csoRepository;
        }

        public async Task<MethodResult<CSOModel>> Handle(GetCsoByUserIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            MethodResult<CSOModel> methodResult = new MethodResult<CSOModel>();
            var cso = await _csoRepository.GetIncludeByUserIdAsync(request.UserId);
            methodResult.Result = _mapper.Map<CSOModel>(cso);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
