// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.UserOtpCodeQuery
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Hosting;

    public class CheckOtpCommand : IRequest<MethodResult<bool>>
    {
        public string? Otp { get; set; }
        public string? Email { get; set; }
    }

    public class CheckOtpCommandHandler : IRequestHandler<CheckOtpCommand, MethodResult<bool>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IHostEnvironment _environment;
        private readonly IUserOtpRepository _userOtpCodeRepository;
        private readonly string _otpDefault = "123456";

        public CheckOtpCommandHandler(UserManager<User> userManager
            , IHostEnvironment environment
            , IUserOtpRepository userOtpCodeRepository)
        {
            _userManager = userManager;
            _environment = environment;
            _userOtpCodeRepository = userOtpCodeRepository;
        }

        public async Task<MethodResult<bool>> Handle(CheckOtpCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var user = await _userManager.Users.Include(x => x.UserOtpCodes)
                               .FirstOrDefaultAsync(x => x.UserOtpCodes.Any(x => x.Status == EnumUserOtpStatus.New && x.Otp == request.Otp), cancellationToken);
            if (!string.IsNullOrEmpty(request.Email) && request.Otp == _otpDefault && (_environment.IsDevelopment() || _environment.IsEnvironment(Settings.Environments.Testing)))
            {
                if (!request.Email.IsValidEmail())
                {
                    methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.EmailIsNotValid), nameof(request.Email));
                    return methodResult;
                }
                user = await _userManager.Users.FirstOrDefaultAsync(x => !x.IsDeleted && x.Email == request.Email, cancellationToken);
            }
            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.InvalidOTP), nameof(request.Otp), request.Otp);
                return methodResult;
            }

            var userOtpCode = await _userOtpCodeRepository.Queryable
                       .FirstOrDefaultAsync(x => x.UserId == user.Id && x.Status == EnumUserOtpStatus.New && !x.IsDeleted, cancellationToken);
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

            methodResult.Result = true;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
