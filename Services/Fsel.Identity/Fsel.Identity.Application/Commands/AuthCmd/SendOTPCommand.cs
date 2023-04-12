// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.AuthCmd
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Identity.Application.Services;
    using MediatR;

    public class SendOTPCommand : IRequest<MethodResult<bool>>
    {
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Otp { get; set; }
        public string? FullName { get; set; }
    }

    public class SendOTPCommandHandler : IRequestHandler<SendOTPCommand, MethodResult<bool>>
    {
        private readonly ISenderService _senderService;

        public SendOTPCommandHandler(ISenderService senderService)
        {
            _senderService = senderService;
        }

        public async Task<MethodResult<bool>> Handle(SendOTPCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            if (!string.IsNullOrEmpty(request.Email))
            {
                var senderCommandModel = new SendEmailCommandModel
                {
                    Content = string.Format(CultureInfo.InvariantCulture, StringValues.SendOtpContent, request.FullName, request.Otp!),
                    Subject = StringValues.SendOtpSubject + $"{request.Otp!}",
                    ToEmails = new List<string> { $"{request.Email}" }
                };
                var sendResult1 = await _senderService.SendEmailAsync(senderCommandModel);
                if (!sendResult1.IsSuccessStatusCode)
                {
                    methodResult.StatusCode = (int)sendResult1.StatusCode;
                    methodResult.AddError(sendResult1.Content?.ErrorMessages);
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
