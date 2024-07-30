// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.UserConfigs
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.System.Domain.IRepositories;
    using global::System;
    using global::System.Threading.Tasks;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetViewNewFeaturesByUserQuery : IRequest<MethodResult<bool>>
    {
    }

    public class GetViewNewFeaturesByUserQueryHandler : IRequestHandler<GetViewNewFeaturesByUserQuery, MethodResult<bool>>
    {
        private readonly IUserConfigRepository _userConfigRepository;
        private readonly AuthContext _authContext;

        public GetViewNewFeaturesByUserQueryHandler(IUserConfigRepository userConfigRepository, AuthContext authContext)
        {
            _userConfigRepository = userConfigRepository;
            _authContext = authContext;
        }

        public async Task<MethodResult<bool>> Handle(GetViewNewFeaturesByUserQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();
            var isView = await _userConfigRepository.Queryable.AnyAsync(p => p.UserId == _authContext.CurrentUserId && p.IsViewNewFeature, cancellationToken);
            methodResult.Result = isView;
            return methodResult;
        }
    }
}
