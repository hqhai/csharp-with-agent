// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.CSOQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetCsoByUserIdsQuery : IRequest<MethodResult<IList<CSOModel>>>
    {
        public IList<Guid>? UserIds { get; set; }
    }

    public class GetCsoByUserIdsQueryHandler : IRequestHandler<GetCsoByUserIdsQuery, MethodResult<IList<CSOModel>>>
    {
        private readonly IMapper _mapper;
        private readonly ICSORepository _csoRepository;

        public GetCsoByUserIdsQueryHandler(IMapper mapper, ICSORepository csoRepository)
        {
            _mapper = mapper;
            _csoRepository = csoRepository;
        }

        public async Task<MethodResult<IList<CSOModel>>> Handle(GetCsoByUserIdsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var methodResult = new MethodResult<IList<CSOModel>>();
            if (request.UserIds == null || !request.UserIds.Any())
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            var cso = await _csoRepository.Queryable.Include(x => x.User).Where(x => request.UserIds.Contains(x.UserId))
                            .ToListAsync(cancellationToken);

            methodResult.Result = _mapper.Map<IList<CSOModel>>(cso);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
