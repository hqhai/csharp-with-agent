// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.TokenHistoryQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System;
    using global::System.Linq;
    using global::System.Threading.Tasks;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetUserCurrentTokenQuery : IRequest<MethodResult<SearchTokenHistoryModel>>
    {
    }

    public class GetUserCurrentTokenQueryHandler : IRequestHandler<GetUserCurrentTokenQuery, MethodResult<SearchTokenHistoryModel>>
    {
        private readonly ITokenHistoryRepository _tokenHistoryRepository;
        private readonly IMapper _mapper;

        public GetUserCurrentTokenQueryHandler(ITokenHistoryRepository tokenHistoryRepository, IMapper mapper)
        {
            _tokenHistoryRepository = tokenHistoryRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<SearchTokenHistoryModel>> Handle(GetUserCurrentTokenQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<SearchTokenHistoryModel>();

            var reciveToken = _tokenHistoryRepository.Queryable
                    .Where(x => x.Type == EnumTokenHistoryType.Exchanged)
                    .GroupBy(x => x.UserId)
                    .Select(x => new TokenHistoryModel
                    {
                        ReciveToken = x.Sum(x => x.VolatileToken),
                    }).FirstOrDefault();

            var usedToken = _tokenHistoryRepository.Queryable
                   .Where(x => x.Type == EnumTokenHistoryType.Recevived)
                   .GroupBy(x => x.UserId)
                   .Select(x => new TokenHistoryModel
                   {
                       UsedToken = x.Sum(x => x.VolatileToken),
                   }).FirstOrDefault();

            var tokenConfigs = await _tokenHistoryRepository.Queryable
                   .GroupBy(x => x.UserId)
                   .Select(x => new SearchTokenHistoryModel
                   {
                       ReciveToken = reciveToken!.ReciveToken,
                       UsedToken = usedToken!.UsedToken,
                   })
                        .FirstOrDefaultAsync(cancellationToken);

            methodResult.Result = _mapper.Map<SearchTokenHistoryModel>(tokenConfigs);
            return methodResult;
        }
    }
}
