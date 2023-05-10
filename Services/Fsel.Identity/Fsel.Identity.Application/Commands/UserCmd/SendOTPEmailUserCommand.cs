// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.UserCmd
{
    using System;
    using System.Globalization;
    using System.Text;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Identity.Application.Commands.AuthCmd;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Identity.Infrastructure.ValueSettings;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using OtpNet;

    public class SendOTpEmailUserCommand : IRequest<MethodResult<bool>>
    {
        public string? Email { get; set; }
    }

    public class SendOTpEmailUserCommandHandler : IRequestHandler<SendOTpEmailUserCommand, MethodResult<bool>>
    {
        private readonly UserManager<User> _userManager;
        private readonly AuthContext _authContext;
        private readonly IUserOtpCodeRepository _userOtpCodeRepository;
        private readonly IMediator _mediator;
        private readonly AppSetting _appSetting;
        private readonly IMapper _mapper;

        public SendOTpEmailUserCommandHandler(UserManager<User> userManager,
            AuthContext authContext,
            IUserOtpCodeRepository userOtpCodeRepository,
            IMediator mediator,
            AppSetting appSetting,
            IMapper mapper)
        {
            _userManager = userManager;
            _authContext = authContext;
            _userOtpCodeRepository = userOtpCodeRepository;
            _mediator = mediator;
            _appSetting = appSetting;
            _mapper = mapper;
        }

        public async Task<MethodResult<bool>> Handle(SendOTpEmailUserCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();
            var user = await _userManager.FindByIdAsync(_authContext.CurrentUserId.ToString());
            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumUserErrorCode.UserNotExist));
                return methodResult;
            }
            var userOtpCode = await _userOtpCodeRepository.Queryable
                                  .FirstOrDefaultAsync(x => x.UserId == user.Id && x.Status == EnumStatusUser.New && !x.IsDeleted, cancellationToken);

            var randomSecure = new RandomSecureHelper();
            var totp = new Totp(Encoding.UTF8.GetBytes(randomSecure.Secretstrings()));
            var otp = totp.ComputeTotp();
            if (userOtpCode == null)
            {
                userOtpCode = new UserOtpCode
                {
                    UserId = user.Id,
                    OTPCode = otp,
                    Status = EnumStatusUser.New,
                    ExpiredTime = DateTime.Now.AddDays(_appSetting!.Otp!.StepDayWithAdmin)
                };
                _userOtpCodeRepository.Add(userOtpCode);
                await _userOtpCodeRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            var content = string.Format(CultureInfo.InvariantCulture, StringValues.SendOtpContent, user.FullName, otp);
            var subject = StringValues.SendOtpSubject + $"{otp}";
            var sendResult = new MethodResult<bool>();
            if (request != null && request.Email != null)
            {
                sendResult = await _mediator.Send(new SendOTPCommand { Email = request.Email, Content = content, Subject = subject }, cancellationToken).ConfigureAwait(false);
            }

            if (!sendResult.IsOK)
            {
                methodResult.AddErrorBadRequest(sendResult?.ErrorMessages);
                return methodResult;
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = true;
            return methodResult;
        }
    }
}
