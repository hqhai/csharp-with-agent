// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.TokenConfigQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Infrastructure.Common;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetTokenQuery : IRequest<MethodResult<int>>
    {
        public EnumTokenFeature Feature { get; set; }
        public EnumTokenMission Mission { get; set; }
        public bool IsSuperFireMode { get; set; }
        public Guid? ObjectId { get; set; }
        public int? Level { get; set; }
    }

    public class GetTokenQueryHandler : IRequestHandler<GetTokenQuery, MethodResult<int>>
    {
        private readonly ITokenConfigRepository _tokenConfigRepository;
        private readonly TokenConfigConverter _tokenConfigConverter;

        public GetTokenQueryHandler(ITokenConfigRepository tokenConfigRepository, TokenConfigConverter tokenConfigConverter)
        {
            _tokenConfigRepository = tokenConfigRepository;
            _tokenConfigConverter = tokenConfigConverter;
        }

        public async Task<MethodResult<int>> Handle(GetTokenQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<int>();
            var tokenConfig = await _tokenConfigRepository.Queryable.FirstOrDefaultAsync(x => x.Feature == request.Feature && x.Mission == request.Mission, cancellationToken);
            if (tokenConfig == null)
            {
                return methodResult;
            }
            methodResult.Result = _tokenConfigConverter.GetTotalCorrectByAnswerType(request.IsSuperFireMode ? tokenConfig.SuperConfig : tokenConfig.Config, request.ObjectId, request.Level);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
