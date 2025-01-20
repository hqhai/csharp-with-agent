// Copyright (c) Atlantic. All rights reserved.

using System.Globalization;
using Fsel.Common.ActionResults;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Common.Helpers;
using Fsel.Core.Base.Managers;
using Fsel.Identity.Application.Commands.UserOtpCodeCmd;
using Fsel.Identity.Application.Services;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.Enums;
using Fsel.Identity.Domain.Enums.ErrorCodes;
using Fsel.Identity.Domain.IRepositories;
using Fsel.Identity.Domain.Models.EntityModels;
using Fsel.Identity.Infrastructure.ValueSettings;
using Fsel.Shared.Constants;
using Fsel.Shared.Enums;
using Fsel.Shared.Enums.ErrorCodes;
using Fsel.Shared.Helpers;
using Fsel.Shared.Models.SenderTemplates;
using Fsel.Shared.Models.ShareModels;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Identity.Application.Commands.AuthCmd
{
    public class ForgotPasswordCommand : IRequest<MethodResult<ForgotPasswordResultModel>>
    {
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
    }

    public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, MethodResult<ForgotPasswordResultModel>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IMediator _mediator;
        private readonly AppSetting _appSetting;
        private readonly ISenderService _senderService;
        private readonly IUserOtpCodeRepository _userOtpCodeRepository;

        public ForgotPasswordCommandHandler(UserManager<User> userManager,
                                            IMediator mediator,
                                            AppSetting appSetting,
                                            ISenderService senderService,
                                            IUserOtpCodeRepository userOtpCodeRepository)
        {
            _userManager = userManager;
            _mediator = mediator;
            _appSetting = appSetting;
            _senderService = senderService;
            _userOtpCodeRepository = userOtpCodeRepository;
        }

        public async Task<MethodResult<ForgotPasswordResultModel>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ForgotPasswordResultModel> methodResult = new MethodResult<ForgotPasswordResultModel>();

            if (string.IsNullOrEmpty(request.Email) && string.IsNullOrEmpty(request.PhoneNumber))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Email));
                return methodResult;
            }

            if (!string.IsNullOrEmpty(request.Email) && !request.Email.IsValidEmail())
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.EmailIsNotValid), nameof(request.Email));
                return methodResult;
            }

            if (!string.IsNullOrEmpty(request.PhoneNumber) && !request.PhoneNumber.IsValidPhoneNumber())
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.EmailIsNotValid), nameof(request.Email));
                return methodResult;
            }

            User? user = null;

            if (!string.IsNullOrEmpty(request.Email))
            {
                user = await _userManager.Users.Include(p => p.UserOtpCodes).Include(x => x.Human).FirstOrDefaultAsync(x => x.Email == request.Email, cancellationToken);
            }
            else if (!string.IsNullOrEmpty(request.PhoneNumber))
            {
                user = await _userManager.Users.Include(p => p.UserOtpCodes).Include(x => x.Human).FirstOrDefaultAsync(x => x.PhoneNumber == request.PhoneNumber, cancellationToken);
            }

            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request), request.PhoneNumber ?? request.Email);
                return methodResult;
            }

            var countOTPSMS = user.UserOtpCodes.Where(p => p.Type == EnumUserOtpCodeType.SMS).Count();
            if (countOTPSMS >= _appSetting.SenderConfig?.MaxSendOtpSms && !string.IsNullOrEmpty(request.PhoneNumber))
            {
                methodResult.AddErrorBadRequest(nameof(EnumOTPCodeErrorCode.AttemptsExhausted), nameof(request.PhoneNumber), request.PhoneNumber);
                return methodResult;
            }

            if ((!user.PhoneNumberConfirmed || !user.EmailConfirmed) && user.Human == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(user));
                return methodResult;
            }


            if (!string.IsNullOrEmpty(request.Email))
            {
                var userOtpCode = await _mediator.Send(new SaveUserOtpCodeCommand { Id = user.Id }, cancellationToken);
                var param = new SendOtpTemplateModel
                {
                    OtpCode = userOtpCode.Result,
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

                countOTPSMS = 0;
            }

            else if (!string.IsNullOrEmpty(request.PhoneNumber))
            {
                var otp = NumberHelper.GetRandomCode();
                var userOtpCode = new UserOtpCode
                {
                    UserId = user.Id,
                    OTPCode = otp,
                    Status = EnumOtpCodeStatus.New,
                    Type = EnumUserOtpCodeType.SMS,
                    ExpiredTime = DateTime.MaxValue,
                };
                _userOtpCodeRepository.Add(userOtpCode);
                await _userOtpCodeRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                var sendSMSResult = await _senderService.SendSMSAsync(new SendSMSCommandModel()
                {
                    PhoneNumbers = new List<string> { request.PhoneNumber },
                    Template = EnumSendSMSTemplate.SendOTP,
                    Params = new
                    {
                        OTP = otp
                    }
                });

                countOTPSMS = countOTPSMS + 1;
            }

            methodResult.Result = new ForgotPasswordResultModel { IsSuccess = true, CountOTP = countOTPSMS };
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
