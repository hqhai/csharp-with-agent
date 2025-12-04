// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.UserOtpCodeCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class VerifyOTPForUserEventHaNoiCommand : IRequest<MethodResult<bool>>
    {
        public string? PhoneNumber { get; set; }
        public string? OTP { get; set; }
    }

    public class VerifyOTPForUserEventHaNoiCommandHandler : IRequestHandler<VerifyOTPForUserEventHaNoiCommand, MethodResult<bool>>
    {
        private readonly IUserOtpCodeRepository _userOtpCodeRepository;
        private readonly UserManager<User> _userManager;

        public VerifyOTPForUserEventHaNoiCommandHandler(IUserOtpCodeRepository userOtpCodeRepository, UserManager<User> userManager)
        {
            _userOtpCodeRepository = userOtpCodeRepository;
            _userManager = userManager;
        }

        public async Task<MethodResult<bool>> Handle(VerifyOTPForUserEventHaNoiCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            if (string.IsNullOrEmpty(request.PhoneNumber) || string.IsNullOrEmpty(request.OTP))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required), nameof(request.PhoneNumber), request.PhoneNumber);
                return methodResult;
            }

            var user = await _userManager.Users.FirstOrDefaultAsync(p => p.UserName == request.PhoneNumber, cancellationToken);

            if (user == null)
            {
                var isPhoneNumberAlreadyExist = await _userManager.Users.AnyAsync(p => p.PhoneNumber == request.PhoneNumber, cancellationToken);

                if (isPhoneNumberAlreadyExist)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumOTPCodeErrorCode.PhoneNumberAlreadyExist), nameof(request.PhoneNumber), request.PhoneNumber);
                    return methodResult;
                }
                else
                {
                    methodResult.AddErrorBadRequest(nameof(EnumOTPCodeErrorCode.UserDoesNotExist), nameof(request.PhoneNumber), request.PhoneNumber);
                    return methodResult;
                }
            }

            var lastOTP = await _userOtpCodeRepository.Queryable.Where(p => p.UserId == user.Id && p.Type == EnumUserOtpCodeType.SMS).OrderByDescending(p => p.CreatedDate).FirstOrDefaultAsync(cancellationToken);

            if (lastOTP == null || lastOTP.Status == EnumOtpCodeStatus.Verified)
            {
                methodResult.AddErrorBadRequest(nameof(EnumOTPCodeErrorCode.OTPNotSentYet), nameof(request.OTP), request.OTP);
                return methodResult;
            }

            if (lastOTP.OtpCode != request.OTP)
            {
                methodResult.AddErrorBadRequest(nameof(EnumOTPCodeErrorCode.WrongOTP), nameof(request.OTP), request.OTP);
                return methodResult;
            }

            await _userOtpCodeRepository.ExecuteTransactionAsync(async () =>
            {
                lastOTP.Status = EnumOtpCodeStatus.Verified;
                _userOtpCodeRepository.Update(lastOTP);
                await _userOtpCodeRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });
            return methodResult;
        }
    }
}
