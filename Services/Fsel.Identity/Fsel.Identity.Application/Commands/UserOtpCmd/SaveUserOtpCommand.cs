// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.UserOtpCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Infrastructure.ValueSettings;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SaveUserOtpCommand : IRequest<MethodResult<string>>
    {
        public Guid Id { get; set; }
        public DateTime? ExpiredTime { get; set; }
    }

    public class SaveUserOtpCommandHandler : IRequestHandler<SaveUserOtpCommand, MethodResult<string>>
    {
        private readonly IUserOtpRepository _userOtpCodeRepository;
        private readonly AppSetting _appSetting;

        public SaveUserOtpCommandHandler(IUserOtpRepository userOtpCodeRepository, AppSetting appSetting)
        {
            _userOtpCodeRepository = userOtpCodeRepository;
            _appSetting = appSetting;
        }

        public async Task<MethodResult<string>> Handle(SaveUserOtpCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<string>();
            var userOtpCode = await _userOtpCodeRepository.Queryable
                                 .FirstOrDefaultAsync(x => x.UserId == request.Id && x.Status == EnumUserOtpStatus.New && !x.IsDeleted, cancellationToken);
            var otp = await GetOtpCode();

            var expiredTime = request.ExpiredTime ?? DateTime.UtcNow.AddMinutes(_appSetting!.Otp!.StepTime);

            if (userOtpCode == null)
            {
                userOtpCode = new UserOtp
                {
                    UserId = request.Id,
                    Otp = otp,
                    Status = EnumUserOtpStatus.New,
                    ExpiredTime = expiredTime
                };
                _userOtpCodeRepository.Add(userOtpCode);
            }
            else
            {
                userOtpCode.Otp = otp;
                userOtpCode.ExpiredTime = expiredTime;
                _userOtpCodeRepository.Update(userOtpCode);
            }
            await _userOtpCodeRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            methodResult.Result = otp;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<string?> GetOtpCode()
        {
            var otp = NumberHelper.GetRandomCode();
            var isUsedOtp = await _userOtpCodeRepository.Queryable.AnyAsync(x => x.Otp == otp && x.Status == EnumUserOtpStatus.New);
            if (!isUsedOtp)
            {
                return otp;
            }
            return await GetOtpCode();
        }
    }
}
