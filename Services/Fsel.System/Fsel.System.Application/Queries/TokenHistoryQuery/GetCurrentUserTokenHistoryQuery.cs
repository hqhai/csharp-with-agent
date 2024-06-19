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

            var tokenHistorys = await _tokenHistoryRepository.Queryable
                .Where(x => x.UserId == userId)
                .GroupBy(x => x.UserId)
                .Select(x => new CurrentUserTokenHistoryModel
                {
                    ReciveToken = x.Sum(x => x.Type == EnumTokenHistoryType.Recevived ? x.VolatileToken : 0),
                    UsedToken = x.Sum(x => x.Type == EnumTokenHistoryType.Exchanged ? x.VolatileToken : 0),
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (tokenHistorys != null)
            {
                tokenHistorys.TotalToken = tokenHistorys.ReciveToken - tokenHistorys.UsedToken;
            }

            methodResult.Result = tokenHistorys;
            return methodResult;
        }
    }
}
