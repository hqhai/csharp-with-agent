
using Fsel.Identity.Domain.Enums;
using  Fsel.Identity.Infrastructure.ValueSettings;
using Fsel.Common.ActionResults;
using Fsel.Common.Helpers;
using Fsel.Identity.Domain.IRepositories;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Fsel.Identity.Domain.Entities;

namespace Fsel.Identity.Application.Commands.UserOtpCmd
{
    public class CreateUserOtpCommand : IRequest<MethodResult<string>>
    {
        public Guid? UserId { get; set; }

        public Guid? VerifyId { get; set; }
    }

    public class CreateUserOtpCommandHandler : IRequestHandler<CreateUserOtpCommand, MethodResult<string>>
    {
        private readonly IUserOtpRepository _userOtpRepository;
        private readonly AppSetting _appSetting;

        public CreateUserOtpCommandHandler(IUserOtpRepository userOtpRepository, AppSetting appSetting)
        {
            _userOtpRepository = userOtpRepository;
            _appSetting = appSetting;
        }

        public async Task<MethodResult<string>> Handle(CreateUserOtpCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<string>();

            if (!request.UserId.HasValue && !request.VerifyId.HasValue)
            {
                methodResult.AddErrorBadRequest(nameof(request.UserId));
                return methodResult;
            }

            var userOtp = await _userOtpRepository.Queryable.FirstOrDefaultAsync(x => (x.UserId == request.UserId || x.VerifyId == request.VerifyId) && x.Status == EnumUserOtpStatus.New && x.ExpiredTime < DateTime.UtcNow, cancellationToken);
            var otp = GenerateHelper.GetOtp();
            if (userOtp == null)
            {
                userOtp = new UserOtp
                {
                    UserId = request.UserId,
                    VerifyId = request.VerifyId,
                    Otp = otp,
                    Status = EnumUserOtpStatus.New,
                    ExpiredTime = DateTime.UtcNow.AddMinutes(_appSetting!.Otp!.StepTime)
                };
                _userOtpRepository.Add(userOtp);
            }
            else
            {
                userOtp.UserId = request.UserId;
                userOtp.VerifyId = request.VerifyId;
                userOtp.Otp = otp;
                userOtp.ExpiredTime = DateTime.UtcNow.AddMinutes(_appSetting!.Otp!.StepTime);
                _userOtpRepository.Update(userOtp);
            }
            await _userOtpRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            methodResult.Result = otp;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

    }
}
