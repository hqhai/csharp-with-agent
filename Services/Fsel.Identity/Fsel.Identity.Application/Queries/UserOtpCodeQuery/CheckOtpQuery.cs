// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.UserOtpCodeQuery
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.Models.CommandModels.UserOtpCodes;
    using Fsel.Identity.Infrastructure.Common;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class CheckOtpQuery : ConfirmOtpCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class CheckOtpQueryHandler : IRequestHandler<CheckOtpQuery, MethodResult<bool>>
    {
        private readonly UserOtpCodeHelper _userOtpCodeHelper;

        public CheckOtpQueryHandler(UserOtpCodeHelper userOtpCodeHelper)
        {
            _userOtpCodeHelper = userOtpCodeHelper;
        }

        public async Task<MethodResult<bool>> Handle(CheckOtpQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();
            var method = await _userOtpCodeHelper.ValidateOtp(request, cancellationToken);
            if (!method.IsOK)
            {
                methodResult.AddErrorBadRequest(method.ErrorMessages);
                return methodResult;
            }
            methodResult.Result = true;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
