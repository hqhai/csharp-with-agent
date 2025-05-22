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

        public ConfirmOtpCommandHandler(IUserOtpCodeRepository userOtpCodeRepository,
                                        IMapper mapper,
                                        UserManager<User> userManager)
        {
            _userOtpCodeRepository = userOtpCodeRepository;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<MethodResult<UserOtpCodeModel>> Handle(ConfirmOtpCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<UserOtpCodeModel> methodResult = new MethodResult<UserOtpCodeModel>();

            UserOtpCode? userOtpCode = new UserOtpCode();

            if (request.UserId.HasValue)
            {
                var user = await _userManager.Users.FirstOrDefaultAsync(p => p.Id == request.UserId, cancellationToken);
                if (user == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumOTPCodeErrorCode.UserDoesNotExist), nameof(request.UserId), request.UserId);
                    return methodResult;
                }

                request.Email = user.Email;
            }

            userOtpCode = await _userOtpCodeRepository.GetUserOtpCodeAsync(request.Otp, request.Email, request.PhoneNumber, (!request.Email.IsNullOrEmpty() ? EnumUserOtpCodeType.Email : EnumUserOtpCodeType.SMS));
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

            await _userOtpCodeRepository.ExecuteTransactionAsync(async () =>
            {
                userOtpCode.Status = EnumOtpCodeStatus.Verified;
                _userOtpCodeRepository.Update(userOtpCode);

                if (!string.IsNullOrEmpty(request.Email))
                {
                    var userTypeSMS = await _userOtpCodeRepository.Queryable
                                                                  .FirstOrDefaultAsync(p => p.Type == EnumUserOtpCodeType.SMS && p.Status == EnumOtpCodeStatus.New && p.User != null && p.User.Email == request.Email.Trim(), cancellationToken);

                    if (userTypeSMS != null)
                    {
                        userTypeSMS.Status = EnumOtpCodeStatus.Verified;
                        _userOtpCodeRepository.Update(userTypeSMS);
                    }
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
