// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.UserOtpCodeCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.UserOtpCodes;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Identity.Infrastructure.Common;
    using Fsel.Identity.Infrastructure.ValueSettings;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Kros.Extensions;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ConfirmOtpCommand : ConfirmOtpCommandModel, IRequest<MethodResult<UserOtpCodeModel>>
    {
    }

    public class ConfirmOtpCommandHandler : IRequestHandler<ConfirmOtpCommand, MethodResult<UserOtpCodeModel>>
    {
        private readonly IUserOtpCodeRepository _userOtpCodeRepository;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly SaveOtpCodeConverter _saveOtpCodeConverter;

        public ConfirmOtpCommandHandler(IUserOtpCodeRepository userOtpCodeRepository,
                                        IMapper mapper,
                                        UserManager<User> userManager,
                                        SaveOtpCodeConverter saveOtpCodeConverter)
        {
            _userOtpCodeRepository = userOtpCodeRepository;
            _mapper = mapper;
            _userManager = userManager;
            _saveOtpCodeConverter = saveOtpCodeConverter;
        }

        public async Task<MethodResult<UserOtpCodeModel>> Handle(ConfirmOtpCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<UserOtpCodeModel> methodResult = new MethodResult<UserOtpCodeModel>();

            UserOtpCode? userOtpCode = new UserOtpCode();
            User? user = null;

            if (request.UserId.HasValue)
            {
                user = await _userManager.Users.FirstOrDefaultAsync(p => p.Id == request.UserId, cancellationToken);
                if (user == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumOTPCodeErrorCode.UserDoesNotExist), nameof(request.UserId), request.UserId);
                    return methodResult;
                }

                request.Email = user.Email;
            }

            if (!request.Email.IsNullOrEmpty())
            {
                user = await _userManager.Users
                                            .Include(p => p.UserOtpCodes)
                                            .FirstOrDefaultAsync(p => p.Email.Trim().ToLower() == request.Email.Trim().ToLower(), cancellationToken);

                if (user == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumOTPCodeErrorCode.UserDoesNotExist), nameof(request.PhoneNumber), request.PhoneNumber);
                    return methodResult;
                }

                var checkOtp = await _saveOtpCodeConverter.CheckOtp(request, user, EnumUserOtpCodeType.Email);
                if (!checkOtp.Result)
                {
                    methodResult.AddError(checkOtp.ErrorMessages.ToList());
                    return methodResult;
                }
                else
                {
                    userOtpCode = user.UserOtpCodes.FirstOrDefault(p => p.Type == EnumUserOtpCodeType.Email && p.Status == EnumOtpCodeStatus.New);
                    if (userOtpCode == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.InvalidOTP), nameof(request.Otp), request.Otp);
                        return methodResult;
                    }
                }
            }

            else if (!request.PhoneNumber.IsNullOrEmpty())
            {
                user = await _userManager.Users
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
                else
                {
                    userOtpCode = user.UserOtpCodes.FirstOrDefault(p => p.Type == EnumUserOtpCodeType.SMS && p.Status == EnumOtpCodeStatus.New);
                    if (userOtpCode == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumOTPCodeErrorCode.OTPNotSentYet), nameof(request.Otp), request.Otp);
                        return methodResult;
                    }
                }
            }

            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumOTPCodeErrorCode.UserDoesNotExist), nameof(request), request);
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
