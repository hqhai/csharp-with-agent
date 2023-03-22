// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Sender.Application.Commands.SendSmsCmd
{
    using Azure.Communication.Sms;
    using Fsel.Common.ActionResults;
    using Fsel.Sender.Domain.Models.Commands;
    using Fsel.Sender.Domain.ValueSettings;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class SmsCommand : SmsCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class LoginCommandHandler : IRequestHandler<SmsCommand, MethodResult<bool>>
    {
        public LoginCommandHandler()
        {
        }

        public async Task<MethodResult<bool>> Handle(SmsCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            string connectionString = Environment.GetEnvironmentVariable("COMMUNICATION_SERVICES_CONNECTION_STRING");
            SmsClient smsClient = new SmsClient(connectionString);
            string from = "<your_twilio_number>"; // Số điện thoại SMS của bạn

            string to = "+84335046326"; // Số điện thoại người nhận, cần có định dạng quốc tế
            var response = await smsClient.SendAsync(from, new[] { to }, request.Content, cancellationToken: cancellationToken);

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = true;
            return methodResult;
        }
    }
}
