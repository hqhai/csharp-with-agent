// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.TokenHistoryQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System;
    using global::System.Linq;
    using global::System.Threading.Tasks;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetCurrentUserTokenHistoryQuery : IRequest<MethodResult<CurrentUserTokenHistoryModel>>
    {
        public Guid? UserId { get; set; }
    }

    public class GetCurrentUserTokenHistoryQueryHandler : IRequestHandler<GetCurrentUserTokenHistoryQuery, MethodResult<CurrentUserTokenHistoryModel>>
    {
        private readonly ITokenHistoryRepository _tokenHistoryRepository;
        private readonly AuthContext _authContext;

        public GetCurrentUserTokenHistoryQueryHandler(ITokenHistoryRepository tokenHistoryRepository, AuthContext authContext)
        {
            _tokenHistoryRepository = tokenHistoryRepository;
            _authContext = authContext;
        }

        public async Task<MethodResult<CurrentUserTokenHistoryModel>> Handle(GetCurrentUserTokenHistoryQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<CurrentUserTokenHistoryModel>();

            var userId = request.UserId ?? _authContext.CurrentUserId;

            var reciveToken = _tokenHistoryRepository.Queryable
                    .Where(x => x.Type == EnumTokenHistoryType.Recevived && x.UserId == userId)
                    .GroupBy(x => x.UserId)
                    .Select(x => new TokenHistoryModel
                    {
                        ReciveToken = x.Sum(x => x.VolatileToken),
                    }).ToList();

            var usedToken = _tokenHistoryRepository.Queryable
                   .Where(x => x.Type == EnumTokenHistoryType.Exchanged && x.UserId == userId)
                   .GroupBy(x => x.UserId)
                   .Select(x => new TokenHistoryModel
                   {
                       UsedToken = x.Sum(x => x.VolatileToken),
                   }).ToList();

            var tokenHistorys = await _tokenHistoryRepository.Queryable
                   .Where(x => x.UserId == userId)
                   .Select(x => new CurrentUserTokenHistoryModel
                   {
                       ReciveToken = reciveToken.Select(x => x.ReciveToken).Sum(),
                       UsedToken = usedToken.Select(x => x.UsedToken).Sum(),
                   }).FirstOrDefaultAsync(cancellationToken);

            tokenHistorys!.TotalToken = tokenHistorys.ReciveToken + tokenHistorys.UsedToken;

            methodResult.Result = tokenHistorys;
            return methodResult;
        }
    }
}
