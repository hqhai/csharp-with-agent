// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.AuthCmd
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Identity.Application.Services;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;

    public class SendOTPCommand : IRequest<MethodResult<bool>>
    {
        public string? Email { get; set; }
    }

    public class SendOTPCommandHandler : IRequestHandler<SendOTPCommand, MethodResult<bool>>
    {
        private readonly UserManager<User> _userManager;
        private readonly ISenderService _senderService;

        public SendOTPCommandHandler(UserManager<User> userManager,
            ISenderService senderService)
        {
            _userManager = userManager;
            _senderService = senderService;
        }

        public async Task<MethodResult<bool>> Handle(SendOTPCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();
            var user = await _userManager.FindByEmailAsync(request.Email ?? string.Empty);
            if (user == null)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddError(
                    nameof(EnumAuthErrorCode.AU04ER),
                    new[] { MethodHelper.GenerateErrorResult(nameof(request.Email), request.Email) });
                return methodResult;
            }
            else if (user.EmailConfirmed)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddError(
                    nameof(EnumAuthErrorCode.AU12ER),
                    new[] { MethodHelper.GenerateErrorResult(nameof(request.Email), request.Email) });
                return methodResult;
            }

            if (request.Email != null)
            {
                var otp = await _userManager.GenerateTwoFactorTokenAsync(user, nameof(request.Email));
                var senderCommandModel1 = new SendEmailCommandModel
                {
                    Content = $"Xin chào {user.FullName} \r\n\r\n"
                              + $"Hệ thống giáo dục LMS FSEL xin gửi đến bạn mã xác thực OTP : {otp} "
                              + $"\r\n\r\nMã OTP chỉ có giá trị hiệu lực trong thời gian 3 phút. \r\n\r\nXin chân thành cảm ơn!",
                    Subject = $"\"[LMS -FSEL]: \"",
                    ToEmails = new List<string> { $"{request.Email}" }
                };
                var sendResult1 = await _senderService.SendEmailAsync(senderCommandModel1);
                if (!sendResult1.IsSuccessStatusCode)
                {
                    methodResult.StatusCode = (int)sendResult1.StatusCode;
                    methodResult.AddError(sendResult1.Content?.ErrorMessages);
                    return methodResult;
                }
            }
            else
            {
                //Send PhoneNumber
            }
            return methodResult;
        }
    }
}
