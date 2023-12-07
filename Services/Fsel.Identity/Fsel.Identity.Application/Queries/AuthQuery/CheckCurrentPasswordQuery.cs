// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.AuthQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    using UserManager = Core.Base.Managers.UserManager<Domain.Entities.User>;

    public class CheckCurrentPasswordQuery : IRequest<MethodResult<bool>>
    {
        public string? OldPassword { get; set; }
    }

    public class CheckCurrentPasswordQueryHandler : IRequestHandler<CheckCurrentPasswordQuery, MethodResult<bool>>
    {
        private readonly UserManager _userManager;
        private readonly AuthContext _authContext;

        public CheckCurrentPasswordQueryHandler(UserManager userManager, AuthContext authContext)
        {
            _userManager = userManager;
            _authContext = authContext;
        }

        public async Task<MethodResult<bool>> Handle(CheckCurrentPasswordQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();
            if (request.OldPassword == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.OldPassword));
                return methodResult;
            }
            var user = await _userManager.FindByIdAsync(_authContext.CurrentUserId.ToString());
            if (user == null)
            {
                methodResult.Result = false;
                return methodResult;
            }
            var isCheckPassword = await _userManager.CheckPasswordAsync(user, request.OldPassword);
            if (!isCheckPassword)
            {
                methodResult.Result = false;
                return methodResult;
            }
            methodResult.Result = true;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
