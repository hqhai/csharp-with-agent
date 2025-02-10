// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.UserOtpCodeCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.Enums;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.UserOtpCodes;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Enums;
    using Kros.Extensions;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Domain.Entities;
    using Microsoft.EntityFrameworkCore;

    public class ConfirmOtpCommand : ConfirmOtpCommandModel, IRequest<MethodResult<UserOtpCodeModel>>
    {
    }

    public class ConfirmOtpCommandHandler : IRequestHandler<ConfirmOtpCommand, MethodResult<UserOtpCodeModel>>
    {
        private readonly IUserOtpCodeRepository _userOtpCodeRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ConfirmOtpCommandHandler> _logger;
        private readonly UserManager<User> _userManager;

        public ConfirmOtpCommandHandler(IUserOtpCodeRepository userOtpCodeRepository, IMapper mapper, ILogger<ConfirmOtpCommandHandler> logger, UserManager<User> userManager)
        {
            _userOtpCodeRepository = userOtpCodeRepository;
            _mapper = mapper;
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<MethodResult<UserOtpCodeModel>> Handle(ConfirmOtpCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<UserOtpCodeModel> methodResult = new MethodResult<UserOtpCodeModel>();

            UserOtpCode? userOtpCode = new UserOtpCode();
            User? user = null;

            if (!string.IsNullOrEmpty(request.Email))
            {
                user = await _userManager.Users
                                            .Include(p => p.UserOtpCodes)
                                            .FirstOrDefaultAsync(p => p.Email != null && p.Email.Trim() == request.Email.Trim(), cancellationToken);

                userOtpCode = await _userOtpCodeRepository.GetUserOtpCodeAsync(request.Otp, request.Email);
                if (userOtpCode == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.InvalidOTP), nameof(request.Otp), request.Otp);
                    return methodResult;
                }
                if (request.IsCheckExpiredTime && DateTime.Compare(DateTime.UtcNow, userOtpCode.ExpiredTime) > 0)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.OTPExpired), nameof(request.Otp), request.Otp);
                    return methodResult;
                }
            }
            else if (!string.IsNullOrEmpty(request.PhoneNumber))
            {
                user = await _userManager.Users
                                         .Include(p => p.UserOtpCodes)
                                         .FirstOrDefaultAsync(p => p.UserName != null && p.UserName.Trim() == request.PhoneNumber.Trim(), cancellationToken);

                if (user == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumOTPCodeErrorCode.UserDoesNotExist), nameof(request.PhoneNumber), request.PhoneNumber);
                    return methodResult;
                }

                userOtpCode = user.UserOtpCodes.FirstOrDefault(p => p.Type == EnumUserOtpCodeType.SMS && p.Status == EnumOtpCodeStatus.New);
                if (userOtpCode == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumOTPCodeErrorCode.OTPNotSentYet), nameof(request.Otp), request.Otp);
                    return methodResult;
                }

                if (userOtpCode.OTPCode != request.Otp)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumOTPCodeErrorCode.WrongOTP), nameof(request.Otp), request.Otp);
                    return methodResult;
                }
            }

            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumOTPCodeErrorCode.UserDoesNotExist), nameof(request.PhoneNumber), request.PhoneNumber);
                return methodResult;
            }

            await _userOtpCodeRepository.ExecuteTransactionAsync(async () =>
            {
                userOtpCode.Status = EnumOtpCodeStatus.Verified;
                _userOtpCodeRepository.Update(userOtpCode);

                var userTypeSMS = user.UserOtpCodes.FirstOrDefault(p => p.Type == EnumUserOtpCodeType.SMS && p.Status == EnumOtpCodeStatus.New);
                if (userTypeSMS != null && !request.Email.IsNullOrEmpty())
                {
                    userTypeSMS.Status = EnumOtpCodeStatus.Verified;
                }

                await _userOtpCodeRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.Result = _mapper.Map<UserOtpCodeModel>(userOtpCode);
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            });

            return methodResult;
        }
    }
}
