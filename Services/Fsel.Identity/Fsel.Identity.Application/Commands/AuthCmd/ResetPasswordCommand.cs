using Fsel.Common.ActionResults;
using Fsel.Common.Helpers;
using Fsel.Core.Base;
using Fsel.Identity.Common.Models.Commands;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.Enums.ErrorCodes;
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
        private readonly IMediator _mediator;

        public ResetPasswordCommandHandler(UserManager<User> userManager,
            AuthContext authContext,
            IMediator mediator)
        {
            _authContext = authContext;
            _userManager = userManager;
            _mediator = mediator;
        }

        public async Task<MethodResult<bool>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            MethodResult<bool> methodResult = new MethodResult<bool>();
            if (request.OldPassword == null)
            {
                methodResult.StatusCode = StatusCodes.Status401Unauthorized;
                methodResult.AddErrorMessage(
                    nameof(EnumAuthErrorCode.AU01V),
                    new[] { MethodHelper.GenerateErrorResult(nameof(request.OldPassword), request.OldPassword) });
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

            var user = await _userManager.FindByEmailAsync(request.Email ?? string.Empty);
            if (user == null)
            {
                methodResult.StatusCode = StatusCodes.Status404NotFound;
                methodResult.AddErrorMessage(
                    nameof(EnumAuthErrorCode.AU04V),
                    new[] { MethodHelper.GenerateErrorResult(nameof(request.Email), request.Email) });
                return methodResult;
            }

            var hashPasswordOle = _userManager.PasswordHasher.HashPassword(user, request.OldPassword);
            if (hashPasswordOle != user.PasswordHash)
            {
                methodResult.StatusCode = StatusCodes.Status404NotFound;
                methodResult.AddErrorMessage(
                    nameof(EnumAuthErrorCode.AU05V),
                    new[] { MethodHelper.GenerateErrorResult(nameof(request.OldPassword), request.OldPassword) });
                return methodResult;
            }

            var hashPassword = _userManager.PasswordHasher.HashPassword(user, request.Password);
            user.PasswordHash = hashPassword;
            await _userManager.UpdateAsync(user);

            return methodResult;
        }
    }
}