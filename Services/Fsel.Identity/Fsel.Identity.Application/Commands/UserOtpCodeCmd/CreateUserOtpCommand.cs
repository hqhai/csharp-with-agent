
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
using Fsel.Identity.Infrastructure;
using Fsel.Core.Base.Interfaces;

namespace Fsel.Identity.Application.Commands.UserOtpCodeCmd
{
    public class CreateUserOtpCommand : IRequest<MethodResult<UserOtpCodeModel>>
    {
        public Guid? UserId { get; set; }

        public Guid? VerifyId { get; set; }
    }

    public class CreateUserOtpCommandHandler : IRequestHandler<CreateUserOtpCommand, MethodResult<UserOtpCodeModel>>
    {
        private IUserOtpCodeRepository _userOtpRepository;
        private readonly IMapper _mapper;
        private readonly ITenantProvider _tenantProvider;
        private readonly AppSetting _appSetting;

        public CreateUserOtpCommandHandler(IUserOtpCodeRepository userOtpRepository, AppSetting appSetting, IMapper mapper, ITenantProvider tenantProvider)
        {
            _userOtpRepository = userOtpRepository;
            _appSetting = appSetting;
            _mapper = mapper;
            _tenantProvider = tenantProvider;
        }

        public async Task<MethodResult<UserOtpCodeModel>> Handle(CreateUserOtpCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            _userOtpRepository = await _tenantProvider.CreateRepositoryAsync<IUserOtpCodeRepository>(userId: request.UserId) ?? _userOtpRepository;

            var methodResult = new MethodResult<UserOtpCodeModel>();

            if (!request.UserId.HasValue && !request.VerifyId.HasValue)
            {
                methodResult.AddErrorBadRequest(nameof(request.UserId));
                return methodResult;
            }

            var userOtp = await _userOtpRepository.Queryable
                .OrderByDescending(x => x.UpdatedDate ?? x.CreatedDate)
                .FirstOrDefaultAsync(x => x.UserId == request.UserId || x.VerifyId == request.VerifyId, cancellationToken);

            var otp = GenerateHelper.GetOtp();
            var userOtpNew = _mapper.Map<UserOtpCode>(userOtp) ?? new UserOtpCode();
            userOtpNew.UserId = request.UserId;
            userOtpNew.VerifyId = request.VerifyId;
            userOtpNew.OtpCode = otp;
            userOtpNew.Status = EnumOtpCodeStatus.New;
            userOtpNew.ExpiredTime = DateTime.UtcNow.AddMinutes(_appSetting!.Otp!.StepTime);

            if (userOtp != null && userOtp.Status == EnumOtpCodeStatus.New)
            {
                _userOtpRepository.Update(userOtpNew);
            }
            else
            {
                _userOtpRepository.Add(userOtpNew);
            }
            await _userOtpRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            methodResult.Result = _mapper.Map<UserOtpCodeModel>(userOtpNew);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

    }
}
