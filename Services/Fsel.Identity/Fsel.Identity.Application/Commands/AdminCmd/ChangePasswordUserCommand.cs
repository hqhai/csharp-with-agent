// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.AdminCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.Models.CommandModels.Auths;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using UserManager = Fsel.Core.Base.Managers.UserManager<Domain.Entities.User>;

    public class ChangePasswordUserCommand : ChangePasswordCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class ChangePasswordUserCommandHandler : IRequestHandler<ChangePasswordUserCommand, MethodResult<bool>>
    {
        private readonly UserManager _userManager;

        public ChangePasswordUserCommandHandler(UserManager userManager)
        {
            _userManager = userManager;
        }

        public async Task<MethodResult<bool>> Handle(ChangePasswordUserCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            if (string.IsNullOrEmpty(request.Password))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Password));
                return methodResult;
            }

            if (!request.UserId.HasValue)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.UserId));
                return methodResult;
            }

            var user = await _userManager.FindByIdAsync(request.UserId.Value.ToString());
            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(user));
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
