// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.UserOtpCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.UserOtpCodes;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class ConfirmOtpCommand : ConfirmOtpCommandModel, IRequest<MethodResult<UserOtpModel>>
    {
    }

    public class ConfirmOtpCommandHandler : IRequestHandler<ConfirmOtpCommand, MethodResult<UserOtpModel>>
    {
        private readonly IUserOtpRepository _userOtpRepository;
        private readonly IMapper _mapper;

        public ConfirmOtpCommandHandler(IUserOtpRepository userOtpRepository, IMapper mapper)
        {
            _userOtpRepository = userOtpRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<UserOtpModel>> Handle(ConfirmOtpCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<UserOtpModel> methodResult = new MethodResult<UserOtpModel>();
            var userOtpCode = await _userOtpRepository.GetUserOtpCodeAsync(request.Otp, request.Email);
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
            try
            {
                userOtpCode.Status = EnumUserOtpStatus.Verified;
                _userOtpRepository.Update(userOtpCode);
                await _userOtpRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }
            catch
            {
            }
            methodResult.Result = _mapper.Map<UserOtpModel>(userOtpCode);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
