
using Fsel.Identity.Domain.Enums;
using Fsel.Identity.Infrastructure.ValueSettings;
using Fsel.Common.ActionResults;
using Fsel.Common.Helpers;
using Fsel.Identity.Domain.IRepositories;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Fsel.Identity.Domain.Entities;
using AutoMapper;
using Fsel.Identity.Domain.Models.EntityModels;

namespace Fsel.Identity.Application.Commands.UserOtpCmd
{
    public class CreateUserOtpCommand : IRequest<MethodResult<UserOtpModel>>
    {
        public Guid? UserId { get; set; }

        public Guid? VerifyId { get; set; }
    }

    public class CreateUserOtpCommandHandler : IRequestHandler<CreateUserOtpCommand, MethodResult<UserOtpModel>>
    {
        private readonly IUserOtpRepository _userOtpRepository;
        private readonly IMapper _mapper;
        private readonly AppSetting _appSetting;

        public CreateUserOtpCommandHandler(IUserOtpRepository userOtpRepository, AppSetting appSetting, IMapper mapper)
        {
            _userOtpRepository = userOtpRepository;
            _appSetting = appSetting;
            _mapper = mapper;
        }

        public async Task<MethodResult<UserOtpModel>> Handle(CreateUserOtpCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<UserOtpModel>();

            if (!request.UserId.HasValue && !request.VerifyId.HasValue)
            {
                methodResult.AddErrorBadRequest(nameof(request.UserId));
                return methodResult;
            }

            var userOtp = await _userOtpRepository.Queryable
                .OrderByDescending(x => x.UpdatedDate ?? x.CreatedDate)
                .FirstOrDefaultAsync(x => x.UserId == request.UserId || x.VerifyId == request.VerifyId, cancellationToken);

            var otp = GenerateHelper.GetOtp();
            var userOtpNew = _mapper.Map<UserOtp>(userOtp) ?? new UserOtp();
            userOtpNew.UserId = request.UserId;
            userOtpNew.VerifyId = request.VerifyId;
            userOtpNew.Otp = otp;
            userOtpNew.Status = EnumUserOtpStatus.New;
            userOtpNew.ExpiredTime = DateTime.UtcNow.AddMinutes(_appSetting!.Otp!.StepTime);

            if (userOtp != null && userOtp.Status == EnumUserOtpStatus.New)
            {
                _userOtpRepository.Update(userOtpNew);
            }
            else
            {
                _userOtpRepository.Add(userOtpNew);
            }
            await _userOtpRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            methodResult.Result = _mapper.Map<UserOtpModel>(userOtpNew);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

    }
}
