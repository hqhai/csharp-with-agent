// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.UserOtpCodeCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ConfirmOtpCommand : IRequest<MethodResult<UserOtpCode>>
    {
        public string? Otp { get; set; }
    }

    public class ConfirmOtpCommandHandler : IRequestHandler<ConfirmOtpCommand, MethodResult<UserOtpCode>>
    {
        private readonly IUserOtpCodeRepository _userOtpCodeRepository;

        public ConfirmOtpCommandHandler(IUserOtpCodeRepository userOtpCodeRepository)
        {
            _userOtpCodeRepository = userOtpCodeRepository;
        }

        public async Task<MethodResult<UserOtpCode>> Handle(ConfirmOtpCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<UserOtpCode> methodResult = new MethodResult<UserOtpCode>();
            var userOtpCode = await _userOtpCodeRepository.Queryable
                                   .FirstOrDefaultAsync(x => x.Status == EnumOtpCodeStatus.New && !x.IsDeleted && x.OTPCode == request.Otp, cancellationToken);
            if (userOtpCode == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.InvalidOTP), nameof(request.Otp), request.Otp);
                return methodResult;
            }

            if (DateTime.Compare(DateTime.UtcNow, userOtpCode.ExpiredTime) > 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.OTPExpired), nameof(request.Otp), request.Otp);
                return methodResult;
            }

            userOtpCode.Status = EnumOtpCodeStatus.Verified;
            _userOtpCodeRepository.Update(userOtpCode);
            await _userOtpCodeRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            methodResult.Result = userOtpCode;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
