// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.CSOQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetAllCSOQuery : IRequest<MethodResult<IList<CSOModel>>>
    {
    }

    public class GetAllCSOQueryHandler : IRequestHandler<GetAllCSOQuery, MethodResult<IList<CSOModel>>>
    {
        private readonly ICSORepository _cSORepository;
        private readonly IMapper _mapper;

        public GetAllCSOQueryHandler(ICSORepository cSORepository, IMapper mapper)
        {
            _cSORepository = cSORepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<CSOModel>>> Handle(GetAllCSOQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<CSOModel>>();

            var allCSo = await _cSORepository.Queryable.Include(p => p.Human).ToListAsync(cancellationToken);
            methodResult.Result = _mapper.Map<IList<CSOModel>>(allCSo);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
