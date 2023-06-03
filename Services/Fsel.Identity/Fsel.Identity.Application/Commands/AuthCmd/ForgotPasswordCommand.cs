// Copyright (c) Atlantic. All rights reserved.

using System.Globalization;
using System.Text;
using Fsel.Common.ActionResults;
using Fsel.Common.Constants;
using Fsel.Common.Helpers;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.Enums;
using Fsel.Identity.Domain.Enums.ErrorCodes;
using Fsel.Identity.Domain.IRepositories;
using Fsel.Identity.Infrastructure.ValueSettings;
using Fsel.Shared.Constants;
using Fsel.Shared.Enums;
using Fsel.Shared.Models.SenderTemplates;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OtpNet;

namespace Fsel.Identity.Application.Commands.AuthCmd
{
    public class ForgotPasswordCommand : IRequest<MethodResult<bool>>
    {
        public string Email { get; set; } = string.Empty;
    }

    public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, MethodResult<bool>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IMediator _mediator;
        private readonly IUserOtpCodeRepository _userOtpCodeRepository;
        private readonly AppSetting _appSetting;

        public ForgotPasswordCommandHandler(UserManager<User> userManager
            , IMediator mediator
            , IUserOtpCodeRepository userOtpCodeRepository
            , AppSetting appSetting)
        {
            _userManager = userManager;
            _mediator = mediator;
            _userOtpCodeRepository = userOtpCodeRepository;
            _appSetting = appSetting;
        }

        public async Task<MethodResult<bool>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();
            if (string.IsNullOrEmpty(request.Email))
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthErrorCode.EmailNull), nameof(request.Email), request.Email);
                return methodResult;
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthErrorCode.EmailNotExist), nameof(request.Email), request.Email);
                return methodResult;
            }

            var userOtpCode = await _userOtpCodeRepository.Queryable
                                  .FirstOrDefaultAsync(x => x.UserId == user.Id && x.Status == EnumStatusUser.New && !x.IsDeleted, cancellationToken);

            RandomSecureHelper randomSecure = new RandomSecureHelper();
            var totp = new Totp(Encoding.UTF8.GetBytes(randomSecure.Secretstrings()));
            var otp = totp.ComputeTotp();
            if (userOtpCode == null)
            {
                userOtpCode = new UserOtpCode
                {
                    UserId = user.Id,
                    OTPCode = otp,
                    Status = EnumStatusUser.New,
                    ExpiredTime = DateTime.Now.AddSeconds(_appSetting!.Otp!.StepTime)
                };
                _userOtpCodeRepository.Add(userOtpCode);
                await _userOtpCodeRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            else
            {
                userOtpCode.OTPCode = otp;
                userOtpCode.ExpiredTime = DateTime.Now.AddSeconds(_appSetting!.Otp!.StepTime);
                _userOtpCodeRepository.Update(userOtpCode);
                await _userOtpCodeRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            var param = new SendOtpTemplateModel
            {
                OtpCode = otp,
                AccessLink = string.Format(CultureInfo.InvariantCulture, _appSetting.ConstantUrl!.ConfirmOtpUrl!, otp),
                OtpValidTime = string.Format(CultureInfo.InvariantCulture, SenderSettings.OtpValidMinute, _appSetting!.Otp!.StepTime)
            };
            var subject = string.Format(CultureInfo.InvariantCulture, SenderSettings.SendOtpSubjectFullName, user.FullName);
            var sendResult = new MethodResult<bool>();
            if (!string.IsNullOrEmpty(request.Email))
            {
                sendResult = await _mediator.Send(new SenderCommand { Email = user.Email, Subject = subject, Params = param, Template = EnumSenderTemplate.SendOtpAndLink }, cancellationToken).ConfigureAwait(false);
            }

            if (!sendResult.IsOK)
            {
                methodResult.AddErrorBadRequest(sendResult?.ErrorMessages);
                return methodResult;
            }
            methodResult.Result = true;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
