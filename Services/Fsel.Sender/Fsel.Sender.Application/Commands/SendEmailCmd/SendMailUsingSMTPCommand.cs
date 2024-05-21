// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Sender.Application.Commands.SendEmailCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Sender.Domain.Models.Commands;
    using MediatR;
    using Fsel.Sender.Domain.ValueSettings;
    using Fsel.Sender.Domain.Models.Entities;
    using Microsoft.AspNetCore.Http;
    using Fsel.Common.Helpers;
    using Fsel.Shared.Constants;
    using MimeKit;

    public class SendMailUsingSMTPCommand : SendEmailCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class SendMailUsingSMTPCommandHandler : IRequestHandler<SendMailUsingSMTPCommand, MethodResult<bool>>
    {
        private readonly AppSetting _appSetting;

        public SendMailUsingSMTPCommandHandler(AppSetting appSetting)
        {
            _appSetting = appSetting;
        }

        public async Task<MethodResult<bool>> Handle(SendMailUsingSMTPCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

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
            emailMessage.From.Add(new MailboxAddress(SenderSettings.HostNameCareers, _appSetting?.SmtpGoogle?.From ?? string.Empty));

            if (message.ToEmails != null)
            {
                foreach (var item in message.ToEmails.Where(x => x.IsValidEmail()))
                {
                    emailMessage.To.Add(new MailboxAddress(item, item));
                }
            }

            if (message.BccEmails != null)
            {
                foreach (var item in message.BccEmails.Where(x => x.IsValidEmail()))
                {
                    emailMessage.Bcc.Add(new MailboxAddress(item, item));
                }
            }

            if (message.CcEmails != null)
            {
                foreach (var item in message.CcEmails.Where(x => x.IsValidEmail()))
                {
                    emailMessage.Cc.Add(new MailboxAddress(item, item));
                }
            }

            emailMessage.Subject = message.Subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = message.Content };
            return emailMessage;
        }

        private async Task Send(MimeMessage mailMessage)
        {
            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                try
                {
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                    await client.ConnectAsync(_appSetting?.SmtpGoogle?.SmtpServer ?? string.Empty, _appSetting?.SmtpGoogle?.Port ?? 0, true);
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    await client.AuthenticateAsync(_appSetting?.SmtpGoogle?.Username ?? string.Empty, _appSetting?.SmtpGoogle?.Password ?? string.Empty);
                    await client.SendAsync(mailMessage);
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
