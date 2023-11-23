// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.AuthCmd
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Application.Commands.UserOtpCodeCmd;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Infrastructure.ValueSettings;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ConfirmOtpProfileCommand : IRequest<MethodResult<bool>>
    {
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? OTP { get; set; }
    }

    public class ConfirmOtpProfileCommandHandler : IRequestHandler<ConfirmOtpProfileCommand, MethodResult<bool>>
    {
        private readonly UserManager<User> _userManager;
        private readonly AuthContext _authContext;
        private readonly IMediator _mediator;
        private readonly AppSetting _appSetting;

        public ConfirmOtpProfileCommandHandler(UserManager<User> userManager
            , AuthContext authContext
            , IMediator mediator
            , AppSetting appSetting)
        {
            _userManager = userManager;
            _authContext = authContext;
            _mediator = mediator;
            _appSetting = appSetting;
        }

        public async Task<MethodResult<bool>> Handle(ConfirmOtpProfileCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(_appSetting.Otp);
            var methodResult = new MethodResult<bool>();
            var user = new User();
            if (!string.IsNullOrEmpty(request.Email))
            {
                if (!request.Email.IsValidEmail())
                {
                    methodResult.AddError(nameof(EnumAuthUserErrorCode.EmailIsNotValid), nameof(request.Email));
                    return methodResult;
                }

                user = await _userManager.Users.FirstOrDefaultAsync(x => x.Email == request.Email && x.Id != _authContext.CurrentUserId, cancellationToken: cancellationToken);
                if (user != null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.DuplicateEmail), nameof(request.Email), request.Email);
                    return methodResult;
                }
            }
            if (!string.IsNullOrEmpty(request.PhoneNumber))
            {
                if (!request.PhoneNumber.IsValidPhoneNumber())
                {
                    methodResult.AddError(nameof(EnumAuthUserErrorCode.PhoneNumberIsNotValid), nameof(request.PhoneNumber));
                    return methodResult;
                }
                user = await _userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == request.PhoneNumber && x.Id != _authContext.CurrentUserId, cancellationToken: cancellationToken);
                if (user != null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.DuplicatePhoneNumber), nameof(request.PhoneNumber), request.PhoneNumber);
                    return methodResult;
                }
            }

            user = await _userManager.Users.Include(x => x.Human).FirstOrDefaultAsync(x => x.Id == _authContext.CurrentUserId, cancellationToken);
            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }
            var method = await _mediator.Send(new ConfirmOtpCommand { Otp = request.OTP, Email = user.Email }, cancellationToken);
            if (!method.IsOK)
            {
                methodResult.AddError(method.ErrorMessages);
                return methodResult;
            }
            if (!string.IsNullOrEmpty(request.Email))
            {
                user.Email = request.Email;
                user.Human!.Email = request.Email;
            }
            else if (!string.IsNullOrEmpty(request.PhoneNumber))
            {
                user.PhoneNumber = request.PhoneNumber;
                user.PhoneNumberConfirmed = true;
                user.Human!.PhoneNumber = request.PhoneNumber;
            }
            if (!user.Human!.IsValid())
            {
                methodResult.AddError(user.Human.ErrorMessages);
                return methodResult;
            }
            if (!user.IsValid())
            {
                methodResult.AddError(user.ErrorMessages);
                return methodResult;
            }

            await _userManager.UpdateAsync(user);
            methodResult.Result = true;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
