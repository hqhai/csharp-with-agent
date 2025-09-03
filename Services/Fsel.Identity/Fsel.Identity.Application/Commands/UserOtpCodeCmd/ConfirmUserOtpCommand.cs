// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.UserOtpCodeCmd
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Identity.Domain.Enums;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Fsel.Core.Base.Interfaces;

    public class ConfirmUserOtpCommand : IRequest<MethodResult<bool>>
    {
        public Guid? UserId { get; set; }

        public Guid? VerifyId { get; set; }

        public string? Otp { get; set; }
    }

    public class ConfirmUserOtpCommandHandler : IRequestHandler<ConfirmUserOtpCommand, MethodResult<bool>>
    {
        private IUserOtpCodeRepository _userOtpRepository;
        private readonly ITenantProvider _tenantProvider;

        public ConfirmUserOtpCommandHandler(IUserOtpCodeRepository userOtpRepository, ITenantProvider tenantProvider)
        {
            _userOtpRepository = userOtpRepository;
            _tenantProvider = tenantProvider;
        }

        public async Task<MethodResult<bool>> Handle(ConfirmUserOtpCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            _userOtpRepository = await _tenantProvider.CreateRepositoryAsync<IUserOtpCodeRepository>(userId: request.UserId) ?? _userOtpRepository;

            var methodResult = new MethodResult<bool>();

            var query = _userOtpRepository.Queryable.Where(x => x.Status == EnumOtpCodeStatus.New && x.OtpCode == request.Otp);
            if (request.UserId.HasValue)
            {
                query = query.Where(x => x.UserId == request.UserId);
            }
            if (request.VerifyId.HasValue)
            {
                query = query.Where(x => x.VerifyId == request.VerifyId);
            }

            var userOtp = await query.FirstOrDefaultAsync(cancellationToken);
            if (userOtp == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumUserOtpCodeErrorCode.OtpInvalid), nameof(request.Otp), request.Otp);
                return methodResult;
            }

            if (DateTime.Compare(DateTime.UtcNow, userOtp.ExpiredTime) > 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumUserOtpCodeErrorCode.OtpExpired), nameof(request.Otp), request.Otp);
                return methodResult;
            }

            userOtp.Status = EnumOtpCodeStatus.Verified;
            _userOtpRepository.Update(userOtp);
            await _userOtpRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            methodResult.Result = true;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
