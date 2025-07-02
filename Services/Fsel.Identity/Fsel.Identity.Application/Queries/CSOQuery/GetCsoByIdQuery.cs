// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.CSOQuery
{
    using System;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetCsoByIdQuery : IRequest<MethodResult<CSOModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetCsoByIdQueryHandler : IRequestHandler<GetCsoByIdQuery, MethodResult<CSOModel>>
    {
        private readonly IMapper _mapper;
        private readonly ICSORepository _csoRepository;

        public GetCsoByIdQueryHandler(IMapper mapper, ICSORepository csoRepository)
        {
            _mapper = mapper;
            _csoRepository = csoRepository;
        }

        public async Task<MethodResult<CSOModel>> Handle(GetCsoByIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            MethodResult<CSOModel> methodResult = new MethodResult<CSOModel>();
            var cso = await _csoRepository.Queryable.Include(p => p.User).FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            methodResult.Result = _mapper.Map<CSOModel>(cso);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
