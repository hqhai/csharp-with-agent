// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.SenderCmd
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fsel.Identity.Application.Services.SenderService;
    using Fsel.Common.ActionResults;
    using MediatR;
    using Refit;
    using Fsel.Shared.Enums;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Identity.Infrastructure.ValueSettings;
    using Microsoft.Extensions.DependencyInjection;
    using Fsel.Core.Extensions;

    public class SendOtpCommand : IRequest<MethodResult<bool>>
    {
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Content { get; set; }
        public string? Subject { get; set; }
        public object? Params { get; set; }
        public EnumSenderTemplate? Template { get; set; }
    }

    public class SendOtpCommandHandler : IRequestHandler<SendOtpCommand, MethodResult<bool>>
    {
        private ISenderService _senderService;
        private readonly IServiceProvider _serviceProvider;
        private readonly AppSetting _appSetting;

        public SendOtpCommandHandler(ISenderService senderService, IServiceProvider serviceProvider, AppSetting appSetting)
        {
            _senderService = senderService;
            _serviceProvider = serviceProvider;
        }

        public async Task<MethodResult<bool>> Handle(SendOtpCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var tenantProvider = _serviceProvider.GetService<ITenantProvider>();
            _senderService = tenantProvider != null ? await tenantProvider.CreateServiceAsync<ISenderService>(_appSetting.Services?.SenderApiUrl, request.Email) ?? _senderService : _senderService;

            MethodResult<bool> methodResult = new MethodResult<bool>();

            if (!string.IsNullOrEmpty(request.Email))
            {
                var senderCommandModel = new SendEmailByTemplateCommandModel
                {
                    Content = request.Content,
                    Subject = request.Subject,
                    Params = request.Params,
                    Template = request.Template,
                    ToEmails = new List<string> { $"{request.Email}" }
                };

                IApiResponse<MethodResult<bool>> sendResult;
                if (request.Template.HasValue)
                {
                    sendResult = await _senderService.SendEmailAsync(senderCommandModel);
                }
                else
                {
                    sendResult = await _senderService.SendEmailAsync((SendEmailCommandModel)senderCommandModel);
                }

                if (!sendResult.IsSuccessStatusCode)
                {
                    methodResult.AddErrorBadRequest(sendResult.Content?.ErrorMessages);
                    return methodResult;
                }
            }
            else if (!string.IsNullOrEmpty(request.PhoneNumber))
            {
                //Send PhoneNumber
            }
            return methodResult;
        }
    }
}
