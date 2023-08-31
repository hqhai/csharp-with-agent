// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.SenderCmd
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Lms.Application.Services.SenderService;
    using Fsel.Shared.Enums;
    using MediatR;
    using Refit;

    public class SenderCommand : IRequest<MethodResult<bool>>
    {
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Content { get; set; }
        public string? Subject { get; set; }
        public object? Params { get; set; }
        public EnumSenderTemplate? Template { get; set; }
    }

    public class SenderCommandHandler : IRequestHandler<SenderCommand, MethodResult<bool>>
    {
        private readonly ISenderService _senderService;

        public SenderCommandHandler(ISenderService senderService)
        {
            _senderService = senderService;
        }

        public async Task<MethodResult<bool>> Handle(SenderCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

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
