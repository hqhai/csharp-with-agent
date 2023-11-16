// Copyright (c) Atlantic. All rights reserved.

using System.Globalization;
using Fsel.Common.ActionResults;
using Fsel.Common.Constants;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Common.Helpers;
using Fsel.Core.Base.Managers;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.Enums;
using Fsel.Identity.Domain.Enums.ErrorCodes;
using Fsel.Identity.Domain.IRepositories;
using Fsel.Identity.Infrastructure.ValueSettings;
using Fsel.Shared.Constants;
using Fsel.Shared.Enums;
using Fsel.Shared.Helpers;
using Fsel.Shared.Models.SenderTemplates;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

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
        private readonly IHostEnvironment _environment;
        private readonly IUserOtpCodeRepository _userOtpCodeRepository;
        private readonly AppSetting _appSetting;

        public ForgotPasswordCommandHandler(UserManager<User> userManager
            , IMediator mediator
            , IHostEnvironment environment
            , IUserOtpCodeRepository userOtpCodeRepository
            , AppSetting appSetting)
        {
            _userManager = userManager;
            _mediator = mediator;
            _environment = environment;
            _userOtpCodeRepository = userOtpCodeRepository;
            _appSetting = appSetting;
        }

        public async Task<MethodResult<bool>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();
            if (string.IsNullOrEmpty(request.Email))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Email));
                return methodResult;
            }
            if (!request.Email.IsValidEmail())
            {
                methodResult.AddError(nameof(EnumAuthUserErrorCode.EmailIsNotValid), nameof(request.Email));
                return methodResult;
            }
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                methodResult.Result = true;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            var userOtpCode = await _userOtpCodeRepository.Queryable
                                  .FirstOrDefaultAsync(x => x.UserId == user.Id && x.Status == EnumOtpCodeStatus.New && !x.IsDeleted, cancellationToken);

            var otp = (_environment.IsDevelopment() || _environment.IsEnvironment(Settings.Environments.Testing)) ? ValueSettings.OtpDefault : NumberHelper.GetRandomCode();
            if (userOtpCode == null)
            {
                userOtpCode = new UserOtpCode
                {
                    UserId = user.Id,
                    OTPCode = otp,
                    Status = EnumOtpCodeStatus.New,
                    ExpiredTime = DateTime.UtcNow.AddMinutes(_appSetting!.Otp!.StepTime)
                };
                _userOtpCodeRepository.Add(userOtpCode);
                await _userOtpCodeRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            else
            {
                userOtpCode.OTPCode = otp;
                userOtpCode.ExpiredTime = DateTime.UtcNow.AddMinutes(_appSetting!.Otp!.StepTime);
                _userOtpCodeRepository.Update(userOtpCode);
                await _userOtpCodeRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            var param = new SendOtpTemplateModel
            {
                OtpCode = otp,
                OtpValidTime = string.Format(CultureInfo.InvariantCulture, SenderSettings.OtpValidMinute, _appSetting!.Otp!.StepTime)
            };
            var subject = string.Format(CultureInfo.InvariantCulture, SenderSettings.SendOtpSubjectFullName, user.FullName);
            var sendResult = new MethodResult<bool>();
            if (!string.IsNullOrEmpty(request.Email))
            {
                sendResult = await _mediator.Send(new SenderCommand { Email = user.Email, Subject = subject, Params = param, Template = EnumSenderTemplate.SendOtp }, cancellationToken).ConfigureAwait(false);
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
