using Fsel.Common.ActionResults;
using Fsel.Sender.Application.Services;
using Fsel.Sender.Common.ConfigSettings;
using Fsel.Sender.Common.Models.Commands;
using Fsel.Sender.Common.Models.Entities;
using MediatR;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Http;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
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
        private readonly AppSetting _appSetting;

        public LoginCommandHandler(IEmailService emailService, AppSetting appSetting)
        {
            _emailService = emailService;
            _appSetting = appSetting;
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
                if (request.BccEmails == null || request.BccEmails.Any(x => x == "string")) { }
                else
                {
                    sendEmail.BccEmails = request.BccEmails;
                }
                if (request.CcEmails == null || request.CcEmails.Any(x => x == "string")) { }
                else
                {
                    sendEmail.CcEmails = request.CcEmails;
                }
                sendEmail.Content = request.Content;

                var emailMessage = CreateEmailMessage(sendEmail);
                Send(emailMessage);
                methodResult.Result = sendEmail;
            }

            #endregion Validation

            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        #region SendMail

        private MimeMessage CreateEmailMessage(SendEmailModel message)
        {
            var emailMessage = new MimeMessage();
            var msg = new MailMessage()
            {
                Subject = message.Subject,
                Body = message.Content,
                IsBodyHtml = true
            };
            if (message.ToEmails == null) return emailMessage;

            foreach (var item in message.ToEmails)
            {
                msg.To.Add(new MailAddress(item));
            }
            if (message.BccEmails != null)
            {
                foreach (var item in message.BccEmails)
                {
                    msg.Bcc.Add(new MailAddress(item));
                }
            }
            if (message.CcEmails != null)
            {
                foreach (var item in message.CcEmails)
                {
                    msg.CC.Add(new MailAddress(item));
                }
            }

            emailMessage = MimeMessage.CreateFromMailMessage(msg);
            emailMessage.From.Add(new MailboxAddress($"Send Email", _appSetting?.Smtp?.From ?? string.Empty));
            return emailMessage;
        }

        private void Send(MimeMessage Mailmessage)
        {
            using var client = new MailKit.Net.Smtp.SmtpClient();
            try
            {
                client.Connect(_appSetting?.Smtp?.SmtpServer ?? string.Empty, _appSetting?.Smtp?.Port ?? 0, true);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                client.Authenticate(_appSetting?.Smtp?.Username ?? string.Empty, _appSetting?.Smtp?.Password ?? string.Empty);
                client.Send(Mailmessage);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                client.Disconnect(true);
                client.Dispose();
            }
        }

        #endregion SendMail
    }
}