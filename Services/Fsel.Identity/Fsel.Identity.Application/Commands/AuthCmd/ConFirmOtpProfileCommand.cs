// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.AuthCmd
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Infrastructure.ValueSettings;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ConfirmOtpProfileCommand : IRequest<MethodResult<bool>>
    {
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? OTP { get; set; }
    }

    public class ConfirmOtpProfileCommandHandler : IRequestHandler<ConfirmOtpProfileCommand, MethodResult<bool>>
    {
        private readonly UserManager<User> _userManager;
        private readonly AuthContext _authContext;
        private readonly IUserOtpCodeRepository _userOtpCodeRepository;
        private readonly AppSetting _appSetting;

        public ConfirmOtpProfileCommandHandler(UserManager<User> userManager
            , AuthContext authContext
            , IUserOtpCodeRepository userOtpCodeRepository
            , AppSetting appSetting)
        {
            _userManager = userManager;
            _authContext = authContext;
            _userOtpCodeRepository = userOtpCodeRepository;
            _appSetting = appSetting;
        }

        public async Task<MethodResult<bool>> Handle(ConfirmOtpProfileCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(_appSetting.Otp);
            var methodResult = new MethodResult<bool>();
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Email == request.Email && x.Id != _authContext.CurrentUserId, cancellationToken: cancellationToken);
            if (user != null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.DuplicateEmail), nameof(request.Email), request.Email);
                return methodResult;
            }
            user = await _userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == request.PhoneNumber && x.Id != _authContext.CurrentUserId, cancellationToken: cancellationToken);
            if (user != null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.DuplicatePhoneNumber), nameof(request.PhoneNumber), request.PhoneNumber);
                return methodResult;
            }

            user = await _userManager.Users.Include(x => x.Human).FirstOrDefaultAsync(x => x.Id == _authContext.CurrentUserId, cancellationToken);
            if (user != null)
            {
                if (!string.IsNullOrEmpty(request.Email))
                {
                    user.Email = request.Email;
                    user.Human!.Email = request.Email;
                }
                else if (!string.IsNullOrEmpty(request.PhoneNumber))
                {
                    user.PhoneNumber = request.PhoneNumber;
                    user.Human!.PhoneNumber = request.PhoneNumber;
                }
                if (!user.Human!.IsValid())
                {
                    methodResult.AddError(user.Human.ErrorMessages);
                    return methodResult;
                }
                if (!user.IsValid())
                {
                    methodResult.AddError(user.ErrorMessages);
                    return methodResult;
                }

                await _userManager.UpdateAsync(user);
                var userOtpCode = await _userOtpCodeRepository.Queryable
                        .FirstOrDefaultAsync(x => x.UserId == user.Id && x.Status == EnumOtpCodeStatus.New && !x.IsDeleted && x.OTPCode == request.OTP, cancellationToken);
                if (userOtpCode == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.InvalidOTP), nameof(request.OTP), request.OTP);
                    return methodResult;
                }

                if (DateTime.Compare(DateTime.UtcNow, userOtpCode.ExpiredTime) > 0)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.OTPExpired), nameof(request.OTP), request.OTP);
                    return methodResult;
                }

                userOtpCode.Status = EnumOtpCodeStatus.Verified;
                _userOtpCodeRepository.Update(userOtpCode);
                await _userOtpCodeRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }
            methodResult.Result = true;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
