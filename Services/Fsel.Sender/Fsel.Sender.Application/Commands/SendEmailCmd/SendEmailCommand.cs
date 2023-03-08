using Fsel.Common.ActionResults;
using Fsel.Sender.Application.Services;
using Fsel.Sender.Common.Models.Commands;
using Fsel.Sender.Common.Models.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Sender.Application.Commands.SendEmailCmd
{
    public class SendEmailCommand : SendEmailCommandModel, IRequest<MethodResult<SendEmailModel>>
    {
    }

    public class LoginCommandHandler : IRequestHandler<SendEmailCommand, MethodResult<SendEmailModel>>
    {
        private readonly IEmailService _emailService;

        public LoginCommandHandler(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task<MethodResult<SendEmailModel>> Handle(SendEmailCommand request, CancellationToken cancellationToken)
        {
            MethodResult<SendEmailModel> methodResult = new MethodResult<SendEmailModel>();

            #region Validation

            if (request == null)
            {
                methodResult.StatusCode = StatusCodes.Status401Unauthorized;
                methodResult.AddErrorMessage("SendEmail Failes");
                return methodResult;
            }
            else
            {
                SendEmailModel sendEmail = new SendEmailModel();
                sendEmail.Subject = request.Subject;
                sendEmail.ToEmails = request.ToEmails;
                sendEmail.BccEmails = request.BccEmails;
                sendEmail.CcEmails = request.CcEmails;
                sendEmail.Content = request.Content;
                await _emailService.SendEmailAsync(sendEmail);
            }

            #endregion Validation

            methodResult.StatusCode = StatusCodes.Status200OK;

            return methodResult;
        }
    }
}