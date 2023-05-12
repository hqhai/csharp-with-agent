// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.AuthCmd
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Application.Services;
    using MediatR;

    public class SenderCommand : IRequest<MethodResult<bool>>
    {
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Content { get; set; }
        public string? Subject { get; set; }
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
                var senderCommandModel = new SendEmailCommandModel
                {
                    Content = request.Content,
                    Subject = request.Subject,
                    ToEmails = new List<string> { $"{request.Email}" }
                };
                var sendResult = await _senderService.SendEmailAsync(senderCommandModel);
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
