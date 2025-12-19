// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.AuthCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Application.Commands.UserOtpCodeCmd;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Infrastructure.ValueSettings;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class ConFirmOtpSignUpSMSCommand : IRequest<MethodResult<Guid>>
    {
        public string? OTP { get; set; }
        public string? PhoneNumber { get; set; }
    }

    public class ConFirmOtpSignUpSMSCommandHandler : IRequestHandler<ConFirmOtpSignUpSMSCommand, MethodResult<Guid>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IMediator _mediator;
        private readonly AppSetting _appSetting;
        private readonly IParentRepository _parentRepository;

        public ConFirmOtpSignUpSMSCommandHandler(UserManager<User> userManager
            , IMediator mediator
            , AppSetting appSetting
            , IParentRepository parentRepository)
        {
            _userManager = userManager;
            _mediator = mediator;
            _appSetting = appSetting;
            _parentRepository = parentRepository;
        }

        public async Task<MethodResult<Guid>> Handle(ConFirmOtpSignUpSMSCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(_appSetting.Otp);
            MethodResult<Guid> methodResult = new MethodResult<Guid>();

            if (string.IsNullOrEmpty(request.PhoneNumber))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.PhoneNumber), request.PhoneNumber);
                return methodResult;
            }
            if (!request.PhoneNumber.IsValidPhoneNumber())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(request.PhoneNumber), request.PhoneNumber);
                return methodResult;
            }

            var method = await _mediator.Send(new ConfirmOtpSMSCommand { Otp = request.OTP, PhoneNumber = request.PhoneNumber }, cancellationToken);
            if (!method.IsOK || method.Result == null)
            {
                methodResult.AddError(method.ErrorMessages);
                return methodResult;
            }
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == method.Result.UserId, cancellationToken);
            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(user));
                return methodResult;
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            await _userManager.ConfirmEmailAsync(user, token);
            if (user.Student == null && user.Parent == null)
            {
                var roles = await _userManager.GetRolesAsync(user);

                user = await UpdateUserAsync(roles, user);
                if (!user.IsValid())
                {
                    methodResult.AddError(user.ErrorMessages);
                    return methodResult;
                }
                await _userManager.UpdateAsync(user);
            }

            methodResult.Result = user.Id;
            return methodResult;
        }

        private async Task<User> UpdateUserAsync(IList<string> roles, User user)
        {
            var currentDate = DateTime.UtcNow;
            var weekNumber = (currentDate.DayOfYear - 1) / 7 + 1;

            if (roles.Contains(EnumRoleRegister.Student.ToString()))
            {
                user.Student = new Student
                {
                    UserId = user.Id,
                    CreatedByParent = false,
                    Occupation = "Student"
                };
            }
            else if (roles.Contains(EnumRoleRegister.Parent.ToString()))
            {
                var stt = await _parentRepository.Queryable.CountAsync();
                user.Parent = new Parent
                {
                    UserId = user.Id,
                };
                user.Code = $"PH_{weekNumber}{stt:0000}";
            }
            return user;
        }
    }
}
