// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.PlatformQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetAllPlatformQuery : IRequest<MethodResult<IList<PlatformModel>>>
    {
    }

    public class GetAllPlatformQueryHandler : IRequestHandler<GetAllPlatformQuery, MethodResult<IList<PlatformModel>>>
    {
        private readonly IMapper _mapper;
        private readonly IPlatformRepository _platformRepository;

        public GetAllPlatformQueryHandler(IMapper mapper, IPlatformRepository platformRepository)
        {
            _mapper = mapper;
            _platformRepository = platformRepository;
        }

        public async Task<MethodResult<IList<PlatformModel>>> Handle(GetAllPlatformQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<PlatformModel>>();
            var platforms = await _platformRepository.Queryable.ToListAsync(cancellationToken);
            methodResult.Result = _mapper.Map<IList<PlatformModel>>(platforms);
            return methodResult;
        }
    }
}
