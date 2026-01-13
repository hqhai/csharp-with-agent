// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.UserOtpCodeCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class VerifyOtpUserToSMSCommand : IRequest<MethodResult<bool>>
    {
        public string? OTP { get; set; }
    }

    public class VerifyOtpUserToSMSCommandHandler : IRequestHandler<VerifyOtpUserToSMSCommand, MethodResult<bool>>
    {
        private readonly IUserOtpCodeRepository _userOtpCodeRepository;
        private readonly UserManager<User> _userManager;
        private readonly AuthContext _authContext;

        public VerifyOtpUserToSMSCommandHandler(IUserOtpCodeRepository userOtpCodeRepository,
            UserManager<User> userManager,
            AuthContext authContext)
        {
            _userOtpCodeRepository = userOtpCodeRepository;
            _userManager = userManager;
            _authContext = authContext;
        }

        public async Task<MethodResult<bool>> Handle(VerifyOtpUserToSMSCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            if (string.IsNullOrEmpty(request.OTP))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.OTP), request.OTP);
                return methodResult;
            }

            var user = await _userManager.Users.FirstOrDefaultAsync(p => p.Id == _authContext.CurrentUserId, cancellationToken);
            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(user), user);
                return methodResult;
            }

            var userOtpCode = await _userOtpCodeRepository.Queryable.Where(p => p.UserId == user.Id && p.Type == EnumUserOtpCodeType.SMS && p.Status == EnumOtpCodeStatus.New)
                                                                    .FirstOrDefaultAsync(cancellationToken);
            if (userOtpCode == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumOTPCodeErrorCode.OTPNotSentYet), nameof(request.OTP), request.OTP);
                return methodResult;
            }

            if (userOtpCode.OtpCode != request.OTP)
            {
                methodResult.AddErrorBadRequest(nameof(EnumOTPCodeErrorCode.WrongOTP), nameof(request.OTP), request.OTP);
                return methodResult;
            }

            await _userOtpCodeRepository.ExecuteTransactionAsync(async () =>
            {
                user.PhoneNumberConfirmed = true;
                await _userManager.UpdateAsync(user);

                userOtpCode.Status = EnumOtpCodeStatus.Verified;
                _userOtpCodeRepository.Update(userOtpCode);
                await _userOtpCodeRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });
            return methodResult;
        }
    }
}
