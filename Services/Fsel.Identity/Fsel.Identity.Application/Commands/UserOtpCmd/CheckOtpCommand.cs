// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.UserOtpCmd
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.UserOtpCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class CheckOtpCommand : ConfirmOtpCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class CheckOtpCommandHandler : IRequestHandler<CheckOtpCommand, MethodResult<bool>>
    {
        private readonly IUserOtpRepository _userOtpCodeRepository;

        public CheckOtpCommandHandler(IUserOtpRepository userOtpCodeRepository)
        {
            _userOtpCodeRepository = userOtpCodeRepository;
        }

        public async Task<MethodResult<bool>> Handle(CheckOtpCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();
            var userOtpCode = await _userOtpCodeRepository.GetUserOtpCodeAsync(request.Otp, request.Email);
            if (userOtpCode == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.InvalidOTP), nameof(request.Otp), request.Otp);
                return methodResult;
            }
            if (request.IsCheckExpiredTime && DateTime.Compare(DateTime.UtcNow, userOtpCode.ExpiredTime) > 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.OTPExpired), nameof(request.Otp), request.Otp);
                return methodResult;
            }
            methodResult.Result = true;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
