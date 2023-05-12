// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.UserCmd
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Infrastructure.ValueSettings;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;

    public class ComfirmOtpProfileCommand : IRequest<MethodResult<bool>>
    {
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? OTP { get; set; }
    }

    public class ComfirmOtpProfileCommandHandler : IRequestHandler<ComfirmOtpProfileCommand, MethodResult<bool>>
    {
        private readonly UserManager<User> _userManager;
        private readonly AuthContext _authContext;
        private readonly IUserOtpCodeRepository _userOtpCodeRepository;
        private readonly AppSetting _appSetting;

        public ComfirmOtpProfileCommandHandler(UserManager<User> userManager
            , AuthContext authContext
            , IUserOtpCodeRepository userOtpCodeRepository
            , AppSetting appSetting)
        {
            _userManager = userManager;
            _authContext = authContext;
            _userOtpCodeRepository = userOtpCodeRepository;
            _appSetting = appSetting;
        }

        public async Task<MethodResult<bool>> Handle(ComfirmOtpProfileCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(_appSetting.Otp);
            var methodResult = new MethodResult<bool>();
            var user = await _userManager.FindByIdAsync(_authContext.CurrentUserId.ToString());

            var userOtpCode = await _userOtpCodeRepository.Queryable
                        .FirstOrDefaultAsync(x => x.UserId == user!.Id && x.Status == EnumStatusUser.New && !x.IsDeleted && x.OTPCode == request.OTP, cancellationToken);
            if (userOtpCode == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthErrorCode.InvalidOTP), nameof(request.OTP), request.OTP);
                return methodResult;
            }

            if (DateTime.Compare(DateTime.Now, userOtpCode.ExpiredTime) > 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthErrorCode.OTPExpired), nameof(request.OTP), request.OTP);
                return methodResult;
            }

            userOtpCode.Status = EnumStatusUser.Verified;
            _userOtpCodeRepository.Update(userOtpCode);
            await _userOtpCodeRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

            if (string.IsNullOrEmpty(request.Email))
            {
                user!.Email = request.Email;
                await _userManager.UpdateAsync(user);
            }
            else if (string.IsNullOrEmpty(request.PhoneNumber))
            {
                user!.PhoneNumber = request.PhoneNumber;
                await _userManager.UpdateAsync(user);
            }

            methodResult.Result = true;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
