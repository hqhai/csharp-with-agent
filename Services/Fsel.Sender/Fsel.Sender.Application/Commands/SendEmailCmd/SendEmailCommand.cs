// Copyright (c) Atlantic. All rights reserved.

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

        public SendEmailCommandHandler(AppSetting appSetting, SESWrapper wrapper)
        {
            _appSetting = appSetting;
            _wrapper = wrapper;
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
                Destination = new Destination
                {
                    BccAddresses = message.BccEmails?.Where(x => x.IsValidEmail()).ToList(),
                    CcAddresses = message.CcEmails?.Where(x => x.IsValidEmail()).ToList(),
                    ToAddresses = message.ToEmails?.Where(x => x.IsValidEmail()).ToList(),
                },
                Message = new Message
                {
                    Body = new Body
                    {
                        Html = new Content
                        {
                            Data = message.Content
                        },
                        //Text = new Content
                        //{
                        //    Data = message.Content,
                        //    Charset = "UTF-8"
                        //}
                    },
                    Subject = new Content
                    {
                        Data = message.Subject
                    }
                }
            };

            return emailRequest;
        }

        public async Task SendEmail(SendEmailRequest emailRequest)
        {
            await _wrapper.SendEmailAsync(emailRequest);
        }
    }
}
