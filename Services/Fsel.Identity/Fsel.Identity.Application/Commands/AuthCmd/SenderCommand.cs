// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.AuthCmd
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Application.Services;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
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
        public bool? IsCCEmailDefault { get; set; }
        public EnumSenderTemplate? Template { get; set; }
    }

    public class SendOTPCommandHandler : IRequestHandler<SenderCommand, MethodResult<bool>>
    {
        private readonly ISenderService _senderService;

        public SendOTPCommandHandler(ISenderService senderService)
        {
            _senderService = senderService;
        }

        public async Task<MethodResult<bool>> Handle(SenderCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            if (!string.IsNullOrEmpty(request.Email))
            {
                var senderCommandModel = new SendEmailByTemplateCommandModel
                {
                    Content = request.Content,
                    Subject = request.Subject,
                    Params = request.Params,
                    Template = request.Template,
                    IsCCEmailDefault = request.IsCCEmailDefault,
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
                    if (sendResult.Content != null)
                    {
                        methodResult.AddErrorBadRequest(sendResult.Content.ErrorMessages);
                    }
                    else
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.SendAuthErorr), sendResult.Error?.Message);
                    }
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
