using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Helpers;
using Fsel.Identity.Application.Services;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.Enums.ErrorCodes;
using Fsel.Sender.Common.Models.Commands;
using Fsel.Sender.Common.Models.Entities;
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
        private readonly IMapper _mapper;
        private readonly SignInManager<User> _signInManager;

        public ForgotPasswordCommandHandler(UserManager<User> userManager, ISenderService senderService,
            IMapper mapper, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _senderService = senderService;
            _mapper = mapper;
            _signInManager = signInManager;
        }

        public async Task<MethodResult<bool>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var user = await _userManager.FindByEmailAsync(request.Email ?? string.Empty);
            if (user == null)
            {
                methodResult.StatusCode = StatusCodes.Status404NotFound;
                methodResult.AddErrorMessage(
                    nameof(EnumAuthErrorCode.AU04V),
                    new[] { MethodHelper.GenerateErrorResult(nameof(request.Email), request.Email) });
                return methodResult;
            }

            var newPassword = new PasswordGeneratorHelper(8, 10).Generate();

            string resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            if (string.IsNullOrEmpty(resetToken))
            {
                methodResult.StatusCode = StatusCodes.Status500InternalServerError;
                methodResult.AddErrorMessage(nameof(EnumAuthErrorCode.AU02ER));
                return methodResult;
            }
            var result = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);
            if (!result.Succeeded)
            {
                methodResult.StatusCode = StatusCodes.Status500InternalServerError;
                methodResult.AddErrorMessage(nameof(EnumAuthErrorCode.AU03ER));
                return methodResult;
            }

            var sender = new SendEmailModel
            {
                Content = $"Tài khoản của bạn đã được reset thành công mời bạn nhập mật khẩu mới :{newPassword}",
                Subject = "Forgot Password ",
                ToEmails = new List<string> { $"{request.Email}" }
            };
            var sendCommand = new SendEmailCommandModel();
            sendCommand.Content = sender.Content;
            sendCommand.Subject = sender.Subject;
            sendCommand.ToEmails = sender.ToEmails;

            var IsSendMail = await _senderService.SendEmailAsync(sendCommand);
            methodResult.Result = true;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}