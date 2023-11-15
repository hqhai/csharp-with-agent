// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.AuthCmd
{
    using System;
    using System.Globalization;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
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

    public class SendOtpProfileCommand : IRequest<MethodResult<bool>>
    {
        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }
    }

    public class SendOTpEmailUserCommandHandler : IRequestHandler<SendOtpProfileCommand, MethodResult<bool>>
    {
        private readonly UserManager<User> _userManager;
        private readonly AuthContext _authContext;
        private readonly IUserOtpCodeRepository _userOtpCodeRepository;
        private readonly IMediator _mediator;
        private readonly AppSetting _appSetting;
        private readonly IHostEnvironment _environment;

        public SendOTpEmailUserCommandHandler(UserManager<User> userManager,
            AuthContext authContext,
            IUserOtpCodeRepository userOtpCodeRepository,
            IMediator mediator,
            AppSetting appSetting,
            IHostEnvironment environment)
        {
            _userManager = userManager;
            _authContext = authContext;
            _userOtpCodeRepository = userOtpCodeRepository;
            _mediator = mediator;
            _appSetting = appSetting;
            _environment = environment;
        }

        public async Task<MethodResult<bool>> Handle(SendOtpProfileCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();
            var user = new User();
            if (!string.IsNullOrEmpty(request.Email))
            {
                if (!request.Email.IsValidEmail())
                {
                    methodResult.AddError(nameof(EnumAuthUserErrorCode.EmailIsNotValid), nameof(request.Email));
                    return methodResult;
                }
                user = await _userManager.Users.FirstOrDefaultAsync(x => x.Email == request.Email && x.Id != _authContext.CurrentUserId, cancellationToken: cancellationToken);
                if (user != null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.DuplicateEmail), nameof(request.Email), request.Email);
                    return methodResult;
                }
            }

            if (!string.IsNullOrEmpty(request.PhoneNumber))
            {
                if (!request.PhoneNumber.IsValidPhoneNumber())
                {
                    methodResult.AddError(nameof(EnumAuthUserErrorCode.PhoneNumberIsNotValid), nameof(request.PhoneNumber));
                    return methodResult;
                }
                user = await _userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == request.PhoneNumber && x.Id != _authContext.CurrentUserId, cancellationToken: cancellationToken);
                if (user != null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.DuplicatePhoneNumber), nameof(request.PhoneNumber), request.PhoneNumber);
                    return methodResult;
                }
            }

            user = await _userManager.FindByIdAsync(_authContext.CurrentUserId.ToString());
            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(user));
                return methodResult;
            }

            var userOtpCode = await _userOtpCodeRepository.Queryable
                                  .FirstOrDefaultAsync(x => x.UserId == user.Id && x.Status == EnumOtpCodeStatus.New && !x.IsDeleted, cancellationToken);
            var otp = (_environment.IsDevelopment() || _environment.IsEnvironment(Settings.Environments.Testing)) ? ValueSettings.OtpDefault : NumberHelper.GetRandomCode();
            if (userOtpCode != null && DateTime.Compare(DateTime.UtcNow, userOtpCode.ExpiredTime) > 0)
            {
                userOtpCode.OTPCode = otp;
                userOtpCode.ExpiredTime = DateTime.UtcNow.AddMinutes(_appSetting!.Otp!.StepTime);
                _userOtpCodeRepository.Update(userOtpCode);
                await _userOtpCodeRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
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
            var param = new SendOtpTemplateModel
            {
                OtpCode = userOtpCode.OTPCode,
                OtpValidTime = string.Format(CultureInfo.InvariantCulture, SenderSettings.OtpValidMinute, _appSetting!.Otp!.StepTime)
            };
            var subject = string.Format(CultureInfo.InvariantCulture, SenderSettings.SendOtpSubjectFullName, user.FullName);
            var sendResult = new MethodResult<bool>();
            if (!string.IsNullOrEmpty(request.Email))
            {
                sendResult = await _mediator.Send(new SenderCommand { Email = user.Email, Subject = subject, Params = param, Template = EnumSenderTemplate.SendOtp }, cancellationToken).ConfigureAwait(false);
            }
            else if (!string.IsNullOrEmpty(request.PhoneNumber))
            {
                sendResult = await _mediator.Send(new SenderCommand { Email = user.Email, Subject = subject, Params = param, Template = EnumSenderTemplate.SendOtp }, cancellationToken).ConfigureAwait(false);
            }

            if (!sendResult.IsOK)
            {
                methodResult.AddErrorBadRequest(sendResult?.ErrorMessages);
                return methodResult;
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = true;
            return methodResult;
        }
    }
}
