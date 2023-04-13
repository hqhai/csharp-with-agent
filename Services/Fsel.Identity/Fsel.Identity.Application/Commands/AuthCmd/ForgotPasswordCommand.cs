// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ActionResults;
using Fsel.Common.Helpers;
using Fsel.Identity.Application.Services;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.Enums.ErrorCodes;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Fsel.Identity.Application.Commands.AuthCmd
{
    public class ForgotPasswordCommand : IRequest<MethodResult<bool>>
    {
        public string Email { get; set; } = string.Empty;
    }

    public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, MethodResult<bool>>
    {
        private readonly UserManager<User> _userManager;
        private readonly ISenderService _senderService;

        public ForgotPasswordCommandHandler(UserManager<User> userManager, ISenderService senderService)
        {
            _userManager = userManager;
            _senderService = senderService;
        }

        public async Task<MethodResult<bool>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();
            var user = await _userManager.FindByEmailAsync(request.Email ?? string.Empty);
            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthErrorCode.EmailNotExist), nameof(request.Email), request.Email);
                return methodResult;
            }

            var newPassword = new PasswordGeneratorHelper(8, 10).Generate();

            string resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            if (string.IsNullOrEmpty(resetToken))
            {
                methodResult.StatusCode = StatusCodes.Status500InternalServerError;
                methodResult.AddError(nameof(EnumAuthErrorCode.ErrorResetToken));
                return methodResult;
            }
            var result = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);
            if (!result.Succeeded)
            {
                methodResult.StatusCode = StatusCodes.Status500InternalServerError;
                methodResult.AddError(nameof(EnumAuthErrorCode.ErrorResetPassword));
                return methodResult;
            }

            var sendCommandModel = new SendEmailCommandModel
            {
                Content = $"Tài khoản của bạn đã được reset thành công mời bạn nhập mật khẩu mới :{newPassword}",
                Subject = "Forgot Password ",
                ToEmails = new List<string> { $"{request.Email}" }
            };

            var isSendMail = await _senderService.SendEmailAsync(sendCommandModel);

            if (isSendMail.IsSuccessStatusCode)
            {
                methodResult.StatusCode = StatusCodes.Status500InternalServerError;
                methodResult.AddError(nameof(EnumAuthErrorCode.ErrorSendEmail));
                return methodResult;
            }

            methodResult.Result = true;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
