// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.UserOtpCodeCmd
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Models.CommandModels.UserOtpCodes;
    using Fsel.Identity.Infrastructure.Common;
    using Fsel.Identity.Infrastructure.ValueSettings;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Kros.Extensions;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CheckOtpCommand : ConfirmOtpCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class CheckOtpCommandHandler : IRequestHandler<CheckOtpCommand, MethodResult<bool>>
    {
        private readonly UserManager<User> _userManager;
        private readonly SaveOtpCodeConverter _saveOtpCodeConverter;

        public CheckOtpCommandHandler(UserManager<User> userManager,
                                      SaveOtpCodeConverter saveOtpCodeConverter)
        {
            _userManager = userManager;
            _saveOtpCodeConverter = saveOtpCodeConverter;
        }

        public async Task<MethodResult<bool>> Handle(CheckOtpCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            if (!request.Email.IsNullOrEmpty())
            {
                var user = await _userManager.Users
                                             .Include(p => p.UserOtpCodes)
                                             .FirstOrDefaultAsync(p => p.Email.Trim().ToLower() == request.Email.Trim().ToLower(), cancellationToken);
                if (user == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumOTPCodeErrorCode.UserDoesNotExist), nameof(request.Email), request.Email);
                    return methodResult;
                }

                var checkOtp = await _saveOtpCodeConverter.CheckOtp(request, user, EnumUserOtpCodeType.Email);
                if (!checkOtp.Result)
                {
                    methodResult.AddError(checkOtp.ErrorMessages.ToList());
                    return methodResult;
                }
            }

            else if (!request.PhoneNumber.IsNullOrEmpty())
            {
                var user = await _userManager.Users
                                             .Include(p => p.UserOtpCodes)
                                             .FirstOrDefaultAsync(p => p.UserName.Trim().ToLower() == request.PhoneNumber.Trim().ToLower(), cancellationToken);

                if (user == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumOTPCodeErrorCode.UserDoesNotExist), nameof(request.PhoneNumber), request.PhoneNumber);
                    return methodResult;
                }

                var checkOtp = await _saveOtpCodeConverter.CheckOtp(request, user, EnumUserOtpCodeType.SMS);
                if (!checkOtp.Result)
                {
                    methodResult.AddError(checkOtp.ErrorMessages.ToList());
                    return methodResult;
                }
            }

            methodResult.Result = true;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }


    }
}
