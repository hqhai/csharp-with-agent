// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.AuthCmd
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums;
    using Fsel.Identity.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CheckOtpCommand : IRequest<MethodResult<bool>>
    {
        public string? Otp { get; set; }
    }

    public class CheckOtpCommandHandler : IRequestHandler<CheckOtpCommand, MethodResult<bool>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IUserOtpCodeRepository _userOtpCodeRepository;

        public CheckOtpCommandHandler(UserManager<User> userManager
            , IUserOtpCodeRepository userOtpCodeRepository)
        {
            _userManager = userManager;
            _userOtpCodeRepository = userOtpCodeRepository;
        }

        public async Task<MethodResult<bool>> Handle(CheckOtpCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var user = await _userManager.Users.Include(x => x.UserOtpCodes)
                               .FirstOrDefaultAsync(x => x.UserOtpCodes.Any(x => x.Status == EnumOtpCodeStatus.New && x.OTPCode == request.Otp), cancellationToken);

            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Otp));
                return methodResult;
            }

            var userOtpCode = await _userOtpCodeRepository.Queryable
                       .FirstOrDefaultAsync(x => x.UserId == user!.Id && x.Status == EnumOtpCodeStatus.New && !x.IsDeleted && x.OTPCode == request.Otp, cancellationToken);
            if (userOtpCode == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Otp));
                return methodResult;
            }

            if (DateTime.Compare(DateTime.UtcNow, userOtpCode.ExpiredTime) > 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Otp));
                return methodResult;
            }

            methodResult.Result = true;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
