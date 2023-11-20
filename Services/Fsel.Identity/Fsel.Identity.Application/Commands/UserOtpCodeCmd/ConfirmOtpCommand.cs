// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.UserOtpCodeCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Common.Helpers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Hosting;

    public class ConfirmOtpCommand : IRequest<MethodResult<UserOtpCode>>
    {
        public string? Otp { get; set; }
        public string? Email { get; set; }
    }

    public class ConfirmOtpCommandHandler : IRequestHandler<ConfirmOtpCommand, MethodResult<UserOtpCode>>
    {
        private readonly IUserOtpCodeRepository _userOtpCodeRepository;
        private readonly IHostEnvironment _environment;

        public ConfirmOtpCommandHandler(IUserOtpCodeRepository userOtpCodeRepository, IHostEnvironment environment)
        {
            _userOtpCodeRepository = userOtpCodeRepository;
            _environment = environment;
        }

        public async Task<MethodResult<UserOtpCode>> Handle(ConfirmOtpCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<UserOtpCode> methodResult = new MethodResult<UserOtpCode>();
            var userOtpCode = await _userOtpCodeRepository.Queryable
                                   .FirstOrDefaultAsync(x => x.Status == EnumOtpCodeStatus.New && !x.IsDeleted && x.OTPCode == request.Otp, cancellationToken);
            if (!string.IsNullOrEmpty(request.Email) && (_environment.IsDevelopment() || _environment.IsEnvironment(Settings.Environments.Testing)))
            {
                if (!request.Email.IsValidEmail())
                {
                    methodResult.AddError(nameof(EnumAuthUserErrorCode.EmailIsNotValid), nameof(request.Email));
                    return methodResult;
                }
                userOtpCode = await _userOtpCodeRepository.Queryable.Include(x => x.User)
                                   .FirstOrDefaultAsync(x => x.User != null && x.Status == EnumOtpCodeStatus.New && !x.IsDeleted && x.User.Email == request.Email, cancellationToken);
            }

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
