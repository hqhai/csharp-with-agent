// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ActionResults;
using Fsel.Common.Helpers;
using Fsel.Sender.Domain.Models.Commands;
using Fsel.Sender.Domain.Models.Entities;
using Fsel.Sender.Domain.ValueSettings;
using Fsel.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Http;
using MimeKit;

namespace Fsel.Sender.Application.Commands.SendEmailCmd
{
    public class SendEmailCommand : SendEmailCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class SendEmailCommandHandler : IRequestHandler<SendEmailCommand, MethodResult<bool>>
    {
        private readonly AppSetting _appSetting;

        public SendEmailCommandHandler(AppSetting appSetting)
        {
            _appSetting = appSetting;
        }

        public async Task<MethodResult<bool>> Handle(SendEmailCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            #region Validation

            SendEmailModel sendEmail = new SendEmailModel();
            sendEmail.Subject = request.Subject;
            sendEmail.ToEmails = request.ToEmails;
            sendEmail.CcEmails = request.CcEmails;
            sendEmail.BccEmails = request.BccEmails;
            sendEmail.Content = request.Content;
            try
            {
                using (var emailMessage = CreateEmailMessage(sendEmail))
                {
                    await Send(emailMessage);
                }
            }
            catch (Exception)
            {
                throw;
            }

            #endregion Validation

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = true;
            return methodResult;
        }

        private MimeMessage CreateEmailMessage(SendEmailModel message)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(SenderSettings.HostName, _appSetting?.Smtp?.From ?? string.Empty));

            if (message.ToEmails != null && message.ToEmails.IsValidEmail())
            {
                foreach (var item in message.ToEmails)
                {
                    emailMessage.To.Add(new MailboxAddress(item, item));
                }
            }

            if (message.BccEmails != null && message.BccEmails.IsValidEmail())
            {
                foreach (var item in message.BccEmails)
                {
                    emailMessage.Bcc.Add(new MailboxAddress(item, item));
                }
            }

            if (message.CcEmails != null && message.CcEmails.IsValidEmail())
            {
                foreach (var item in message.CcEmails)
                {
                    emailMessage.Cc.Add(new MailboxAddress(item, item));
                }
            }

            emailMessage.Subject = message.Subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = message.Content };

            return emailMessage;
        }

        private async Task Send(MimeMessage mailmessage)
        {
            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                try
                {
                    await client.ConnectAsync(_appSetting?.Smtp?.SmtpServer ?? string.Empty, _appSetting?.Smtp?.Port ?? 0, true);
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    await client.AuthenticateAsync(_appSetting?.Smtp?.Username ?? string.Empty, _appSetting?.Smtp?.Password ?? string.Empty);
                    await client.SendAsync(mailmessage);
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    await client.DisconnectAsync(true);
                    client.Dispose();
                }
            }
        }
    }
}
