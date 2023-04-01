// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Sender.Application.Commands.SendSmsCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Sender.Domain.Models.Commands;
    using MediatR;

    public class SmsCommand : SmsCommandModel, IRequest<MethodResult<bool>>
    {
    }

    //public class LoginCommandHandler : IRequestHandler<SmsCommand, MethodResult<bool>>
    //{
    //    public LoginCommandHandler()
    //    {
    //    }

    //    public async Task<MethodResult<bool>> Handle(SmsCommand request, CancellationToken cancellationToken)
    //    {
    //        ArgumentNullException.ThrowIfNull(request);
    //        MethodResult<bool> methodResult = new MethodResult<bool>();

    //        methodResult.StatusCode = StatusCodes.Status200OK;
    //        methodResult.Result = true;
    //        return methodResult;
    //    }
    //}
}
