// Copyright (c) Atlantic. All rights reserved.

using Amazon.SimpleEmail.Model;
using Fsel.Common.ActionResults;
using Fsel.Common.Helpers;
using Fsel.Sender.Application.Services;
using Fsel.Sender.Application.Services.SystemServices;
using Fsel.Sender.Domain.Models.Commands;
using Fsel.Sender.Domain.Models.Entities;
using Fsel.Sender.Domain.ValueSettings;
using Fsel.Shared.Enums;
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
        private readonly ISystemService _systemService;
        private readonly IMediator _mediator;

        public SendEmailCommandHandler(AppSetting appSetting, SESWrapper wrapper, ISystemService systemService, IMediator mediator)
        {
            _appSetting = appSetting;
            _wrapper = wrapper;
            _systemService = systemService;
            _mediator = mediator;
        }

        public async Task<MethodResult<bool>> Handle(SendEmailCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var bccEmail = _appSetting.EmailConfig?.BCCEmail;
            if (bccEmail != null && bccEmail.Count > 0)
            {
                bccEmail.ForEach(request.BccEmails.Add);
            }

            #region Get CC Email

            if (request.IsCCEmail.HasValue && request.IsCCEmail.Value)
            {
                var listCCEmailResult = await _systemService.GetCCEmail();
                if (listCCEmailResult.IsSuccessStatusCode)
                {
                    var listCCEmail = listCCEmailResult.Content?.Result;
                    var ccEmail = listCCEmail?.Where(p => !string.IsNullOrEmpty(p.StudentEmail) && request.ToEmails.Contains(p.StudentEmail)).ToList();
                    if (ccEmail != null && ccEmail.Count > 0)
                    {
                        foreach (var item in ccEmail)
                        {
                            if (!string.IsNullOrEmpty(item.OCEmail) && item.OCEmail.IsValidEmail())
                            {
                                request.CcEmails.Add(item.OCEmail);
                            }
                            if (!string.IsNullOrEmpty(item.OMEmail) && item.OMEmail.IsValidEmail())
                            {
                                request.CcEmails.Add(item.OMEmail);
                            }
                        }
                    }
                }
            }

            #endregion Get CC Email

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

                // lưu lại lịch sử gửi mail
                await _mediator.Send(new SaveMessageHistoryByTypeEmailCommand
                {
                    ToEmails = request.ToEmails,
                    CcEmails = request.CcEmails,
                    BccEmails = request.BccEmails,
                    Content = request.Content,
                    Status = EnumMessageHistoryStatus.Success,
                    Template = request.Template
                }, cancellationToken);

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
