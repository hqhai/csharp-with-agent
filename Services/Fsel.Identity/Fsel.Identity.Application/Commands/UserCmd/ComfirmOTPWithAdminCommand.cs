// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.UserCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Application.Commands.AuthCmd;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Identity.Infrastructure.ValueSettings;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;

    public class ComfirmOTPWithAdminCommand : IRequest<MethodResult<ConfirmOtpModel>>
    {
        public string? UserId { get; set; }
        public string? OTP { get; set; }
    }

    public class ComfirmOTPWithAdminCommandHandler : IRequestHandler<ComfirmOTPWithAdminCommand, MethodResult<ConfirmOtpModel>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IMediator _mediator;
        private readonly IUserOtpCodeRepository _userOtpCodeRepository;
        private readonly AppSetting _appSetting;

        public ComfirmOTPWithAdminCommandHandler(UserManager<User> userManager
            , IMediator mediator
            , IUserOtpCodeRepository userOtpCodeRepository
            , AppSetting appSetting)
        {
            _userManager = userManager;
            _mediator = mediator;
            _userOtpCodeRepository = userOtpCodeRepository;
            _appSetting = appSetting;
        }

        public async Task<MethodResult<ConfirmOtpModel>> Handle(ComfirmOTPWithAdminCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(_appSetting.Otp);
            var methodResult = new MethodResult<ConfirmOtpModel>();
            var user = new User();
            if (request.UserId != null)
            {
                user = await _userManager.FindByIdAsync(request.UserId);
            }

            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthErrorCode.UserNotExist), nameof(request.UserId), request.UserId);
                return methodResult;
            }
            var userOtpCode = await _userOtpCodeRepository.Queryable
                        .FirstOrDefaultAsync(x => x.UserId == user.Id && x.Status == EnumStatusUser.New && !x.IsDeleted && x.OTPCode == request.OTP, cancellationToken);
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

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            await _userManager.ConfirmEmailAsync(user, token);

            var generateToken = await _mediator.Send(new GenerateTokenCommand { Id = user.Id }, cancellationToken).ConfigureAwait(false);
            var confirmOtp = new ConfirmOtpModel
            {
                AccessToken = generateToken!.Result!.AccessToken,
                Expiration = generateToken.Result.Expiration,
                FullName = generateToken.Result.FullName,
                RefreshToken = generateToken.Result.RefreshToken,
                Roles = generateToken.Result.Roles,
                UserId = user.Id
            };
            methodResult.Result = confirmOtp;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
