// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.UserOtpCmd
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Identity.Domain.Enums;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ConfirmUserOtpCommand : IRequest<MethodResult<bool>>
    {
        public Guid? UserId { get; set; }

        public Guid? VerifyId { get; set; }

        public string? Otp { get; set; }
    }

    public class ConfirmUserOtpCommandHandler : IRequestHandler<ConfirmUserOtpCommand, MethodResult<bool>>
    {
        private readonly IUserOtpRepository _userOtpRepository;

        public ConfirmUserOtpCommandHandler(IUserOtpRepository userOtpRepository)
        {
            _userOtpRepository = userOtpRepository;
        }

        public async Task<MethodResult<bool>> Handle(ConfirmUserOtpCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();
            var userOtp = await _userOtpRepository.Queryable.FirstOrDefaultAsync(x =>
            (x.UserId == request.UserId || x.VerifyId == request.VerifyId) &&
            x.Status == EnumUserOtpStatus.New &&
            x.Otp == request.Otp, cancellationToken);

            if (userOtp == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumUserOtpErrorCode.OtpInvalid), nameof(request.Otp), request.Otp);
                return methodResult;
            }

            if (DateTime.Compare(DateTime.UtcNow, userOtp.ExpiredTime) > 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumUserOtpErrorCode.OtpExpired), nameof(request.Otp), request.Otp);
                return methodResult;
            }

            userOtp.Status = EnumUserOtpStatus.Verified;
            _userOtpRepository.Update(userOtp);
            await _userOtpRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            methodResult.Result = true;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
