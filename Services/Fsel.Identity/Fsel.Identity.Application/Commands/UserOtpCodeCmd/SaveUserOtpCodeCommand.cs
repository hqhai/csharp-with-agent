// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.UserOtpCodeCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Infrastructure.ValueSettings;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MassTransit.Internals;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    public class SaveUserOtpCodeCommand : IRequest<MethodResult<string>>
    {
        public Guid Id { get; set; }
        public DateTime? ExpiredTime { get; set; }
    }

    public class SaveUserOtpCodeCommandHandler : IRequestHandler<SaveUserOtpCodeCommand, MethodResult<string>>
    {
        private readonly IUserOtpCodeRepository _userOtpCodeRepository;
        private readonly AppSetting _appSetting;
        private readonly ILogger<SaveUserOtpCodeCommandHandler> _logger;

        public SaveUserOtpCodeCommandHandler(IUserOtpCodeRepository userOtpCodeRepository, AppSetting appSetting, ILogger<SaveUserOtpCodeCommandHandler> logger)
        {
            _userOtpCodeRepository = userOtpCodeRepository;
            _appSetting = appSetting;
            _logger = logger;
        }

        public async Task<MethodResult<string>> Handle(SaveUserOtpCodeCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<string> methodResult = new MethodResult<string>();

            try
            {
                var userOtpCode = await _userOtpCodeRepository.Queryable
                                     .FirstOrDefaultAsync(x => x.UserId == request.Id && x.Type == EnumUserOtpCodeType.Email && x.Status == EnumOtpCodeStatus.New, cancellationToken);
                var otp = NumberHelper.GetRandomCode();

                var expiredTime = request.ExpiredTime ?? DateTime.UtcNow.AddMinutes(_appSetting!.Otp!.StepTime);

                if (userOtpCode == null)
                {
                    userOtpCode = new UserOtpCode
                    {
                        UserId = request.Id,
                        OtpCode = otp,
                        Status = EnumOtpCodeStatus.New,
                        Type = EnumUserOtpCodeType.Email,
                        ExpiredTime = expiredTime
                    };
                    _userOtpCodeRepository.Add(userOtpCode);
                }
                else
                {
                    userOtpCode.OtpCode = otp;
                    userOtpCode.ExpiredTime = expiredTime;
                    _userOtpCodeRepository.Update(userOtpCode);
                }
                await _userOtpCodeRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.Result = otp;
                methodResult.StatusCode = StatusCodes.Status200OK;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SaveUserOtpCodeCommand encouters error: {message}", ex.Message);
            }

            return methodResult;
        }

        private async Task<string?> GetOtpCode()
        {
            var otp = NumberHelper.GetRandomCode();
            var isUsedOtp = await _userOtpCodeRepository.Queryable.AnyAsync(x => x.OtpCode == otp && x.Status == EnumOtpCodeStatus.New);
            if (!isUsedOtp)
            {
                return otp;
            }
            return await GetOtpCode();
        }
    }
}
