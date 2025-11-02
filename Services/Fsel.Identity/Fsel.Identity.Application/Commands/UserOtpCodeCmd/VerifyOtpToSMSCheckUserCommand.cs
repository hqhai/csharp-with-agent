// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.UserOtpCodeCmd
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class VerifyOtpToSMSCheckUserCommand : IRequest<MethodResult<Guid>>
    {
        public string? PhoneNumber { get; set; }
        public string? OTP { get; set; }
    }

    public class VerifyOtpToSMSCheckUserCommandHandler : IRequestHandler<VerifyOtpToSMSCheckUserCommand, MethodResult<Guid>>
    {
        private readonly IUserOtpCodeRepository _userOtpCodeRepository;
        private readonly UserManager<User> _userManager;

        public VerifyOtpToSMSCheckUserCommandHandler(IUserOtpCodeRepository userOtpCodeRepository,
            UserManager<User> userManager)
        {
            _userOtpCodeRepository = userOtpCodeRepository;
            _userManager = userManager;
        }

        public async Task<MethodResult<Guid>> Handle(VerifyOtpToSMSCheckUserCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Guid>();

            if (string.IsNullOrEmpty(request.PhoneNumber))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.PhoneNumber));
                return methodResult;
            }

            if (!request.PhoneNumber.IsValidPhoneNumber())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(request.PhoneNumber), request.PhoneNumber);
                return methodResult;
            }

            if (string.IsNullOrEmpty(request.OTP))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.OTP), request.OTP);
                return methodResult;
            }

            var user = await _userManager.Users.FirstOrDefaultAsync(p => p.PhoneNumber == request.PhoneNumber, cancellationToken);
            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.PhoneNumber), request.PhoneNumber);
                return methodResult;
            }

            var userOtpCode = await _userOtpCodeRepository.Queryable.Where(p => p.UserId == user.Id && p.Type == EnumUserOtpCodeType.SMS && p.Status == EnumOtpCodeStatus.New)
                                                                    .FirstOrDefaultAsync(cancellationToken);
            if (userOtpCode == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumOTPCodeErrorCode.OTPNotSentYet), nameof(request.OTP), request.OTP);
                return methodResult;
            }

            if (userOtpCode.OTPCode != request.OTP)
            {
                methodResult.AddErrorBadRequest(nameof(EnumOTPCodeErrorCode.WrongOTP), nameof(request.OTP), request.OTP);
                return methodResult;
            }

            if (DateTime.Compare(DateTime.UtcNow, userOtpCode.ExpiredTime) > 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.OTPExpired), nameof(request.OTP), request.OTP);
                return methodResult;
            }

            await _userOtpCodeRepository.ExecuteTransactionAsync(async () =>
            {
                userOtpCode.Status = EnumOtpCodeStatus.Verified;
                _userOtpCodeRepository.Update(userOtpCode);
                await _userOtpCodeRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = user.Id;
                return methodResult;
            });
            return methodResult;
        }
    }
}
