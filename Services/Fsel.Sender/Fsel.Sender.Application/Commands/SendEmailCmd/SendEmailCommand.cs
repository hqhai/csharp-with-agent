// Copyright (c) Atlantic. All rights reserved.

using System.Drawing;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Fsel.Common.ActionResults;
using Fsel.Common.Helpers;
using Fsel.Sender.Application.Services;
using Fsel.Sender.Domain.Models.Commands;
using Fsel.Sender.Domain.Models.Entities;
using Fsel.Sender.Domain.ValueSettings;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Fsel.Sender.Application.Commands.SendEmailCmd
{
    public class SendEmailCommand : SendEmailCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class SendEmailCommandHandler : IRequestHandler<SendEmailCommand, MethodResult<bool>>
    {
        private readonly AppSetting _appSetting;
        private readonly SESWrapper _wrapper;

        public SendEmailCommandHandler(AppSetting appSetting)
        {
            _appSetting = appSetting;
            _wrapper = new SESWrapper(new AmazonSimpleEmailServiceClient(_appSetting?.Smtp?.Username, _appSetting?.Smtp?.Password, region: Amazon.RegionEndpoint.APSoutheast1));
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
                var emailMessage = CreateEmailMessage(sendEmail);
                await SendEmail(emailMessage);
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

        private SendEmailRequest CreateEmailMessage(SendEmailModel message)
        {
            var emailRequest = new SendEmailRequest
            {
                Source = _appSetting?.Smtp?.From,
                Destination = new Destination(),
                Message = new Message
                {
                    Body = new Body
                    {
                        Text = new Content(message.Content)
                    },
                    Subject = new Content(message.Subject)
                }
            };

            if (message.ToEmails != null)
            {
                foreach (var item in message.ToEmails.Where(x => x.IsValidEmail()))
                {
                    emailRequest.Destination.ToAddresses.Add(item);
                }
            }

            if (message.BccEmails != null)
            {
                foreach (var item in message.BccEmails.Where(x => x.IsValidEmail()))
                {
                    emailRequest.Destination.BccAddresses.Add(item);
                }
            }

            if (message.CcEmails != null)
            {
                foreach (var item in message.CcEmails.Where(x => x.IsValidEmail()))
                {
                    emailRequest.Destination.CcAddresses.Add(item);
                }
            }
            return emailRequest;
        }

        //private async Task Send(MimeMessage mailmessage)
        //{
        //    using (var client = new MailKit.Net.Smtp.SmtpClient())
        //    {
        //        try
        //        {
        //            client.ServerCertificateValidationCallback = (s, c, h, e) => true;
        //            await client.ConnectAsync(_appSetting?.Smtp?.SmtpServer ?? string.Empty, _appSetting?.Smtp?.Port ?? 0, true);
        //            client.AuthenticationMechanisms.Remove("XOAUTH2");
        //            await client.AuthenticateAsync(_appSetting?.Smtp?.Username ?? string.Empty, _appSetting?.Smtp?.Password ?? string.Empty);
        //            await client.SendAsync(mailmessage);
        //        }
        //        catch (Exception)
        //        {
        //            throw;
        //        }
        //        finally
        //        {
        //            await client.DisconnectAsync(true);
        //            client.Dispose();
        //        }
        //    }
        //}
        //public void SendEmailAsync(SendEmailRequest request)
        //{
        //    //AWSCredentials aWSCredentials = new BasicAWSCredentials(_appSetting?.Smtp?.Username, _appSetting?.Smtp?.Password);
        //    using (var client = AWSClientFactory.CreateAmazonSimpleEmailServiceClient(_appSetting?.Smtp?.Username, _appSetting?.Smtp?.Password))
        //    {
        //        try
        //        {
        //            var a = client.SendEmail(request);
        //        }
        //        catch (Exception ex)
        //        {
        //        }
        //    }
        //}

        public async Task SendEmail(SendEmailRequest emailRequest)
        {
            await _wrapper.SendEmailAsync(
                 emailRequest.Destination.ToAddresses,
                 emailRequest.Destination.CcAddresses,
                 emailRequest.Destination.BccAddresses,
                 emailRequest.Message.Body.Text.Data,
                 string.Empty,
                 emailRequest.Message.Subject.Data,
                  _appSetting?.Smtp?.From
                 );
        }
    }
}
