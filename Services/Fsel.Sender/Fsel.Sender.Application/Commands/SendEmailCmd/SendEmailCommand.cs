using Fsel.Common.ActionResults;
using Fsel.Sender.Application.Services;
using Fsel.Sender.Domain.Models.Commands;
using Fsel.Sender.Domain.Models.Entities;
using Fsel.Sender.Domain.ValueSettings;
using MediatR;
using Microsoft.AspNetCore.Http;
using MimeKit;

namespace Fsel.Sender.Application.Commands.SendEmailCmd
{
    public class SendEmailCommand : SendEmailCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class LoginCommandHandler : IRequestHandler<SendEmailCommand, MethodResult<bool>>
    {
        private readonly IEmailService _emailService;
        private readonly AppSetting _appSetting;

        public LoginCommandHandler(IEmailService emailService, AppSetting appSetting)
        {
            _emailService = emailService;
            _appSetting = appSetting;
        }

        public async Task<MethodResult<bool>> Handle(SendEmailCommand request, CancellationToken cancellationToken)
        {
            MethodResult<bool> methodResult = new MethodResult<bool>();

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
                if (request.ToEmails == null || request.ToEmails.Any(e => e == "string"))
                { }
                else
                {
                    sendEmail.ToEmails = request.ToEmails;
                }
                if (request.BccEmails == null || request.BccEmails.Any(e => e == "string"))
                { }
                else
                {
                    sendEmail.BccEmails = request.BccEmails;
                }
                if (request.CcEmails == null || request.CcEmails.Any(e => e == "string"))
                { }
                else
                {
                    sendEmail.CcEmails = request.CcEmails;
                }
                sendEmail.Content = request.Content;
                var emailMessage = CreateEmailMessageAsync(sendEmail);
                await Send(emailMessage);
            }

            #endregion Validation

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = true;
            return methodResult;
        }

        private MimeMessage CreateEmailMessageAsync(SendEmailModel message)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("email", _appSetting?.Smtp?.From ?? string.Empty));
            if (message.ToEmails == null)
                return emailMessage;

            foreach (var item in message.ToEmails)
            {
                emailMessage.To.Add(new MailboxAddress("email", item));
            }
            if (message.BccEmails != null)
            {
                foreach (var item in message.BccEmails)
                {
                    emailMessage.Bcc.Add(new MailboxAddress("email", item));
                }
            }
            if (message.CcEmails != null)
            {
                foreach (var item in message.CcEmails)
                {
                    emailMessage.Cc.Add(new MailboxAddress("email", item));
                }
            }
            emailMessage.Subject = message.Subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Text) { Text = message.Content };
            return emailMessage;
        }

        private async Task Send(MimeMessage Mailmessage)
        {
            using var client = new MailKit.Net.Smtp.SmtpClient();
            try
            {
                client.Connect(_appSetting?.Smtp?.SmtpServer ?? string.Empty, _appSetting?.Smtp?.Port ?? 0, true);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                client.Authenticate(_appSetting?.Smtp?.Username ?? string.Empty, _appSetting?.Smtp?.Password ?? string.Empty);
                await client.SendAsync(Mailmessage);
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
    }
}
