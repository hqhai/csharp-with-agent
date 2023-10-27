// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.PlatformQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetPlatformsByTypeQuery : IRequest<MethodResult<IList<PlatformModel>>>
    {
        public EnumPlatformType Type { get; set; }
    }

    public class GetPlatformsByTypeQueryHandler : IRequestHandler<GetPlatformsByTypeQuery, MethodResult<IList<PlatformModel>>>
    {
        private readonly IPlatformRepository _platformRepository;
        private readonly IMapper _mapper;
        public GetPlatformsByTypeQueryHandler(IPlatformRepository platformRepository, IMapper mapper)
        {
            _platformRepository = platformRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<PlatformModel>>> Handle(GetPlatformsByTypeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<PlatformModel>>();
            var platforms = await _platformRepository.Queryable.Where(p => p.Type == request.Type).ToListAsync(cancellationToken);
            methodResult.Result = _mapper.Map<IList<PlatformModel>>(platforms);
            return methodResult;
        }
    }
}
