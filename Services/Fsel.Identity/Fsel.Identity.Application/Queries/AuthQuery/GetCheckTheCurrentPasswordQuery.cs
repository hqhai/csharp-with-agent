// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.AuthQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Identity.Domain.Entities;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;

    using UserManager = Core.Base.Managers.UserManager<Domain.Entities.User>;

    public class GetCheckTheCurrentPasswordQuery : IRequest<MethodResult<bool>>
    {
        public string? OldPassword { get; set; }
    }

    public class GetCheckTheCurrentPasswordQueryHandler : IRequestHandler<GetCheckTheCurrentPasswordQuery, MethodResult<bool>>
    {
        private readonly UserManager _userManager;
        private readonly AuthContext _authContext;
        private readonly SignInManager<User> _signInManager;

        public GetCheckTheCurrentPasswordQueryHandler(UserManager userManager, AuthContext authContext, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _authContext = authContext;
            _signInManager = signInManager;
        }

        public async Task<MethodResult<bool>> Handle(GetCheckTheCurrentPasswordQuery request, CancellationToken cancellationToken)
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
            var checkOldPassword = await _signInManager.PasswordSignInAsync(user.UserName ?? string.Empty, request.OldPassword, false, false);
            if (!checkOldPassword.Succeeded)
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
