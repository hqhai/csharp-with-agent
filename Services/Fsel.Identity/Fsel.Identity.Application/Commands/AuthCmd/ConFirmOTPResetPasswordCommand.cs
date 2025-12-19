// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ActionResults;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Core.Base.Managers;
using Fsel.Identity.Application.Commands.UserOtpCodeCmd;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.Enums.ErrorCodes;
using Fsel.Identity.Domain.IRepositories;
using Fsel.Identity.Domain.Models.CommandModels.Auths;
using Fsel.Shared.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Identity.Application.Commands.AuthCmd
{
    public class ConfirmOtpResetPasswordCommand : ComfirmOTPResetPasswordCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class ConfirmOtpResetPasswordCommandHandler : IRequestHandler<ConfirmOtpResetPasswordCommand, MethodResult<bool>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IParentRepository _parentRepository;
        private readonly IMediator _mediator;

        public ConfirmOtpResetPasswordCommandHandler(UserManager<User> userManager, IMediator mediator, IParentRepository parentRepository)
        {
            _userManager = userManager;
            _mediator = mediator;
            _parentRepository = parentRepository;
        }

        public async Task<MethodResult<bool>> Handle(ConfirmOtpResetPasswordCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            if (string.IsNullOrEmpty(request.NewPassword))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.NewPassword));
                return methodResult;
            }
            if (string.IsNullOrEmpty(request.Otp))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Otp));
                return methodResult;
            }

            var method = await _mediator.Send(new ConfirmOtpCommand { Otp = request.Otp, Email = request.Email, PhoneNumber = request.PhoneNumber, UserId = request.UserId, IsCheckExpiredTime = false }, cancellationToken);
            if (!method.IsOK || method.Result == null)
            {
                methodResult.AddErrorBadRequest(method.ErrorMessages);
                return methodResult;
            }
            var user = await _userManager.Users.Include(x => x.Student).FirstOrDefaultAsync(x => x.Id == method.Result.UserId, cancellationToken);
            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(user));
                return methodResult;
            }
            if (!user.EmailConfirmed && (user.Student == null || user.Parent == null || user.CSO == null || user.Teacher == null))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(user));
                return methodResult;
            }

            var passwordValidator = new Microsoft.AspNetCore.Identity.PasswordValidator<User>();
            var validPassword = await passwordValidator.ValidateAsync(_userManager, user, request.NewPassword);
            if (!validPassword.Succeeded)
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.PasswordIsNotValid));
                return methodResult;
            }

            var hashPassword = _userManager.PasswordHasher.HashPassword(user, request.NewPassword);
            user.PasswordHash = hashPassword;
            await _userManager.UpdateAsync(user);

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = true;
            return methodResult;
        }

        private async Task<User> UpdateUserAsync(IList<string> roles, User user)
        {
            var currentDate = DateTime.UtcNow;
            var weekNumber = (currentDate.DayOfYear - 1) / 7 + 1;

            if (roles.Contains(EnumRoleRegister.Student.ToString()))
            {
                user.Student = new Student
                {
                    UserId = user.Id,
                    CreatedByParent = false,
                    Occupation = "Student"
                };
            }
            else if (roles.Contains(EnumRoleRegister.Parent.ToString()))
            {
                var stt = await _parentRepository.Queryable.CountAsync();
                user.Parent = new Parent
                {
                    UserId = user.Id,
                };
                user.Code = $"PH_{weekNumber}{stt:0000}";
            }
            return user;
        }
    }
}
