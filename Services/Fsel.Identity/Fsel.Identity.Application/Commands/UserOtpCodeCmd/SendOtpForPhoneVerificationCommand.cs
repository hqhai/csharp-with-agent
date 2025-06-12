// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.UserOtpCodeCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Identity.Domain.Enums;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Identity;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Application.Services;
    using Microsoft.EntityFrameworkCore;
    using Fsel.Core.Base;
    using System.Linq.Dynamic.Core;
    using Fsel.Identity.Domain.Models.CommandModels.UserOtpCodes;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using Microsoft.AspNetCore.Http;
    using Fsel.Shared.Constants;

    public class SendOtpForPhoneVerificationCommand : IRequest<MethodResult<SaveOTPForUserEventHaNoiCommandModel>>
    {
        public bool IsSMS { get; set; }
    }

    public class SendOtpForPhoneVerificationCommandHandler : IRequestHandler<SendOtpForPhoneVerificationCommand, MethodResult<SaveOTPForUserEventHaNoiCommandModel>>
    {
        private readonly IUserOtpCodeRepository _userOtpCodeRepository;
        private readonly UserManager<User> _userManager;
        private readonly ISenderService _senderService;
        private readonly AuthContext _authContext;

        public SendOtpForPhoneVerificationCommandHandler(IUserOtpCodeRepository userOtpCodeRepository,
            UserManager<User> userManager,
            ISenderService senderService,
            AuthContext authContext)
        {
            _userOtpCodeRepository = userOtpCodeRepository;
            _userManager = userManager;
            _senderService = senderService;
            _authContext = authContext;
        }

        public async Task<MethodResult<SaveOTPForUserEventHaNoiCommandModel>> Handle(SendOtpForPhoneVerificationCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<SaveOTPForUserEventHaNoiCommandModel>();

            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == _authContext.CurrentUserId, cancellationToken);
            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(user));
                return methodResult;
            }

            var userOtpCode = await _userOtpCodeRepository.Queryable.Where(p => p.UserId == user.Id && p.Type == EnumUserOtpCodeType.SMS)
                                                                .Where(x => x.Status == EnumOtpCodeStatus.New)
                                                                .FirstOrDefaultAsync(cancellationToken);

            if (userOtpCode != null && userOtpCode.RetryCount >= ValueSettings.Retrycount)
            {
                methodResult.AddErrorBadRequest(nameof(EnumOTPCodeErrorCode.AttemptsExhausted), nameof(user.PhoneNumber), user.PhoneNumber);
                return methodResult;
            }

            await _userOtpCodeRepository.ExecuteTransactionAsync(async () =>
            {
                if (userOtpCode == null)
                {
                    var otp = NumberHelper.GetRandomCode();
                    userOtpCode = new UserOtpCode
                    {
                        UserId = user.Id,
                        OTPCode = otp,
                        Status = EnumOtpCodeStatus.New,
                        Type = EnumUserOtpCodeType.SMS,
                        RetryCount = 1,
                        ExpiredTime = DateTime.MaxValue,
                    };
                    _userOtpCodeRepository.Add(userOtpCode);
                }
                else
                {
                    userOtpCode.RetryCount += 1;
                    userOtpCode = _userOtpCodeRepository.Update(userOtpCode);
                }

                await _userOtpCodeRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                if (request.IsSMS)
                {
                    var sendSMSResult = await _senderService.SendSMSAsync(new SendSMSCommandModel()
                    {
                        PhoneNumbers = new List<string> { user.PhoneNumber ?? string.Empty },
                        Template = EnumSendSMSTemplate.SendOTP,
                        Params = new
                        {
                            OTP = userOtpCode.OTPCode,
                            CountOTP = userOtpCode.RetryCount
                        },
                        IsCheckDuplicate = false,
                    });
                }
                else
                {
                    var sendSMSResult = await _senderService.SendSMSWithZaloAsync(new SendSMSByZaloCommandModel()
                    {
                        PhoneNumbers = new List<string> { user.PhoneNumber ?? string.Empty },
                        Type = 1,
                        Template = EnumZaloTemplate.OTP,
                        Params = new
                        {
                            otp = userOtpCode.OTPCode
                        },
                        UseUnicode = 0
                    });
                }

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = new SaveOTPForUserEventHaNoiCommandModel { Action = EnumActionSaveOTPForEventHaNoi.Success, CountOTP = userOtpCode.RetryCount };
                return methodResult;
            });
            return methodResult;
        }
    }
}
