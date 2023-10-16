// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ActionResults;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Core.Base;
using Fsel.Core.Base.Managers;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.Models.CommandModels.Auths;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using UserManager = Fsel.Core.Base.Managers.UserManager<Fsel.Identity.Domain.Entities.User>;

namespace Fsel.Identity.Application.Commands.AuthCmd
{
    public class ChangePasswordCommand : ChangePasswordCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, MethodResult<bool>>
    {
        private readonly UserManager _userManager;
        private readonly AuthContext _authContext;
        private readonly SignInManager<User> _signInManager;

        public ChangePasswordCommandHandler(UserManager userManager,
            AuthContext authContext,
            SignInManager<User> signInManager)
        {
            _authContext = authContext;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public async Task<MethodResult<bool>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();
            if (request.OldPassword == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.OldPassword));
                return methodResult;
            }
            if (request.Password == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Password));
                return methodResult;
            }

            var user = await _userManager.FindByIdAsync(_authContext.CurrentUserId.ToString());
            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var checkOldPassword = await _signInManager.PasswordSignInAsync(user.UserName ?? string.Empty, request.OldPassword, false, false);
            if (!checkOldPassword.Succeeded)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.OldPassword));
                return methodResult;
            }

            var hashPassword = _userManager.PasswordHasher.HashPassword(user, request.Password);
            user.PasswordHash = hashPassword;
            await _userManager.UpdateAsync(user);

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = true;
            return methodResult;
        }
    }
}
