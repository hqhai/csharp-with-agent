using Fsel.Common.ActionResults;
using Fsel.Common.Helpers;
using Fsel.Core.Base;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.Enums.ErrorCodes;
using Fsel.Identity.Domain.Models.CommandModels.Auths;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Fsel.Identity.Application.Commands.AuthCmd
{
    public class ResetPasswordCommand : ResetPasswordCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, MethodResult<bool>>
    {
        private readonly UserManager<User> _userManager;

        private readonly AuthContext _authContext;
        private readonly SignInManager<User> _signInManager;

        public ResetPasswordCommandHandler(UserManager<User> userManager,
            AuthContext authContext,
            SignInManager<User> signInManager)
        {
            _authContext = authContext;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public async Task<MethodResult<bool>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            MethodResult<bool> methodResult = new MethodResult<bool>();
            if (request?.OldPassword == null)
            {
                methodResult.StatusCode = StatusCodes.Status401Unauthorized;
                methodResult.AddErrorMessage(
                    nameof(EnumAuthErrorCode.AU01V),
                    new[] { MethodHelper.GenerateErrorResult(nameof(request.OldPassword), request?.OldPassword) });
                return methodResult;
            }
            if (request.Password == null)
            {
                methodResult.StatusCode = StatusCodes.Status404NotFound;
                methodResult.AddErrorMessage(
                    nameof(EnumAuthErrorCode.AU02V),
                    new[] { MethodHelper.GenerateErrorResult(nameof(request.Password), request.Password) });
                return methodResult;
            }
            if (request.ConfirmPassword == null)
            {
                methodResult.StatusCode = StatusCodes.Status404NotFound;
                methodResult.AddErrorMessage(
                    nameof(EnumAuthErrorCode.AU03V),
                    new[] { MethodHelper.GenerateErrorResult(nameof(request.ConfirmPassword), request.ConfirmPassword) });
                return methodResult;
            }

            User? user;
            if (string.IsNullOrEmpty(request.Email))
            {
                user = await _userManager.FindByIdAsync(_authContext.CurrentUserId.ToString());
            }
            else
            {
                user = await _userManager.FindByEmailAsync(request?.Email ?? string.Empty);
            }

            if (user == null)
            {
                methodResult.StatusCode = StatusCodes.Status404NotFound;
                methodResult.AddErrorMessage(
                    nameof(EnumAuthErrorCode.AU04V),
                    new[] { MethodHelper.GenerateErrorResult(nameof(request.Email), request?.Email) });
                return methodResult;
            }

            var checkOldPassword = await _signInManager.PasswordSignInAsync(user.UserName ?? string.Empty, request?.OldPassword, false, false);
            if (!checkOldPassword.Succeeded)
            {
                methodResult.StatusCode = StatusCodes.Status404NotFound;
                methodResult.AddErrorMessage(
                    nameof(EnumAuthErrorCode.AU05V),
                    new[] { MethodHelper.GenerateErrorResult(nameof(request.OldPassword), request?.OldPassword) });
                return methodResult;
            }

            var hashPassword = _userManager.PasswordHasher.HashPassword(user, request?.Password ?? string.Empty);
            user.PasswordHash = hashPassword;
            await _userManager.UpdateAsync(user);

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = true;
            return methodResult;
        }
    }
}
