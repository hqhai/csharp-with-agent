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
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    public class ConfirmOtpCommand : ConfirmOtpCommandModel, IRequest<MethodResult<UserOtpCodeModel>>
    {
    }

    public class ConfirmOtpCommandHandler : IRequestHandler<ConfirmOtpCommand, MethodResult<UserOtpCodeModel>>
    {
        private readonly IUserOtpCodeRepository _userOtpCodeRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ConfirmOtpCommandHandler> _logger;

        public ConfirmOtpCommandHandler(IUserOtpCodeRepository userOtpCodeRepository, IMapper mapper, ILogger<ConfirmOtpCommandHandler> logger)
        {
            _userOtpCodeRepository = userOtpCodeRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<MethodResult<UserOtpCodeModel>> Handle(ConfirmOtpCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<UserOtpCodeModel> methodResult = new MethodResult<UserOtpCodeModel>();
            var userOtpCode = await _userOtpCodeRepository.GetUserOtpCodeAsync(request.Otp, request.Email);
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
                userOtpCode.Status = EnumOtpCodeStatus.Verified;
                _userOtpCodeRepository.Update(userOtpCode);
                await _userOtpCodeRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ConfirmOtpCommand encouters error: {message}", ex.Message);
                methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.SendAuthErorr));
                //scope.Dispose();
            }
            methodResult.Result = _mapper.Map<UserOtpCodeModel>(userOtpCode);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
