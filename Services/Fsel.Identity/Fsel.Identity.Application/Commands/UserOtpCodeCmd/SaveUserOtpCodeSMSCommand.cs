// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.UserOtpCodeCmd
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Application.Services.SenderService;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Infrastructure.ValueSettings;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    public class SaveUserOtpCodeSMSCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
        public bool IsSMS { get; set; }
    }

    public class SaveUserOtpCodeSMSCommandHandler : IRequestHandler<SaveUserOtpCodeSMSCommand, MethodResult<bool>>
    {
        private readonly IUserOtpCodeRepository _userOtpCodeRepository;
        private readonly AppSetting _appSetting;
        private readonly ILogger<SaveUserOtpCodeSMSCommandHandler> _logger;
        private readonly ISenderService _senderService;
        private readonly UserManager<User> _userManager;

        public SaveUserOtpCodeSMSCommandHandler(IUserOtpCodeRepository userOtpCodeRepository,
            AppSetting appSetting,
            ILogger<SaveUserOtpCodeSMSCommandHandler> logger,
            ISenderService senderService,
            UserManager<User> userManager)
        {
            _userOtpCodeRepository = userOtpCodeRepository;
            _appSetting = appSetting;
            _logger = logger;
            _senderService = senderService;
            _userManager = userManager;
        }

        public async Task<MethodResult<bool>> Handle(SaveUserOtpCodeSMSCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(user), request.Id.ToString());
                return methodResult;
            }
            try
            {
                var userOtpCode = await _userOtpCodeRepository.Queryable.Where(x => x.UserId == request.Id && x.Type == EnumUserOtpCodeType.SMS)
                                                              .FirstOrDefaultAsync(x => x.Status == EnumOtpCodeStatus.New, cancellationToken);
                if (userOtpCode == null)
                {
                    var otp = NumberHelper.GetRandomCode();
                    userOtpCode = new UserOtpCode
                    {
                        UserId = request.Id,
                        OtpCode = otp,
                        Status = EnumOtpCodeStatus.New,
                        Type = EnumUserOtpCodeType.SMS,
                        RetryCount = 1,
                        ExpiredTime = DateTime.UtcNow.AddMinutes(5),
                    };
                    _userOtpCodeRepository.Add(userOtpCode);
                }
                else
                {
                    userOtpCode.RetryCount += 1;
                    userOtpCode.ExpiredTime = DateTime.UtcNow.AddMinutes(5);
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
                            OTP = userOtpCode.OtpCode,
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
                            otp = userOtpCode.OtpCode
                        },
                        UseUnicode = 0
                    });
                }

                methodResult.Result = true;
                methodResult.StatusCode = StatusCodes.Status200OK;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SaveUserOtpCodeSMSCommand encouters error: {message}", ex.Message);
            }

            return methodResult;
        }
    }
}
