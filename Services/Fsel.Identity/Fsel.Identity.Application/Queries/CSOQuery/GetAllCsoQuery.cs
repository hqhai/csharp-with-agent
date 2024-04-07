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

    public class GetAllCsoQuery : IRequest<MethodResult<IList<CSOModel>>>
    {
    }

    public class GetAllCsoQueryHandler : IRequestHandler<GetAllCsoQuery, MethodResult<IList<CSOModel>>>
    {
        private readonly ICSORepository _csoRepository;
        private readonly IMapper _mapper;

        public GetAllCsoQueryHandler(ICSORepository csoRepository, IMapper mapper)
        {
            _csoRepository = csoRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<CSOModel>>> Handle(GetAllCsoQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<CSOModel>>();

            var allCSo = await _csoRepository.Queryable.Include(x => x.User).ToListAsync(cancellationToken);
            methodResult.Result = _mapper.Map<IList<CSOModel>>(allCSo);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
