// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ActionResults;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Core.Base;
using Fsel.Identity.Domain.Enums.ErrorCodes;
using Fsel.Identity.Domain.Models.CommandModels.Auths;
using MediatR;
using Microsoft.AspNetCore.Http;
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

        public ChangePasswordCommandHandler(UserManager userManager,
            AuthContext authContext)
        {
            _authContext = authContext;
            _userManager = userManager;
        }

        public async Task<MethodResult<bool>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();
            if (string.IsNullOrEmpty(request.OldPassword))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.OldPassword));
                return methodResult;
            }
            if (string.IsNullOrEmpty(request.Password))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Password));
                return methodResult;
            }
            if (request.Password == request.OldPassword)
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.NewPasswordMatchOldPassword), nameof(request.Password));
                return methodResult;
            }

            var user = await _userManager.FindByIdAsync(_authContext.CurrentUserId.ToString());
            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(user));
                return methodResult;
            }

            var isCheckPassword = await _userManager.CheckPasswordAsync(user, request.OldPassword);
            if (!isCheckPassword)
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.OldPasswordIncorrect), nameof(request.OldPassword));
                return methodResult;
            }

            var passwordValidator = new Microsoft.AspNetCore.Identity.PasswordValidator<Domain.Entities.User>();
            var validPassword = await passwordValidator.ValidateAsync(_userManager, user, request.Password);
            if (!validPassword.Succeeded)
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.PasswordIsNotValid));
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
