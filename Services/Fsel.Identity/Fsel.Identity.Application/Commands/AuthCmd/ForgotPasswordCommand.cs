using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Helpers;
using Fsel.Identity.Application.Services;
using Fsel.Identity.Common.Models.Entities;
using Fsel.Identity.Domain.Entities;
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

        public ForgotPasswordCommandHandler(UserManager<User> userManager, ISenderService senderService,
            IMapper mapper)
        {
            _userManager = userManager;
            _senderService = senderService;
            _mapper = mapper;
        }

        public async Task<MethodResult<bool>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var user = await _userManager.FindByEmailAsync(request.Email ?? string.Empty);
            if (user == null)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddErrorMessage("Email does not exist");
                return methodResult;
            }

            var newPassword = new PasswordGeneratorHelper(8, 10).Generate();
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var resultUser = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (!resultUser.Succeeded)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddErrorMessage("Sign up fail");
                return methodResult;
            }
            var sender = new SendEmailModel
            {
                Content = "",
                Subject = "Maajt ",
                ToEmails = new List<string> { $"{request.Email}" }
            };
            //var sendCommand = new SendMailCommand();
            //sendCommand.Content = sender.Content;
            //sendCommand.Subject = sender.Subject;
            //sendCommand.ToEmails = sender.ToEmails;

            //_senderService.SendEmailModel(sendCommand);

            return methodResult;
        }
    }
}