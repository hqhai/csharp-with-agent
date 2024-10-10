// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.UserOtpCodeCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.UserOtpCodes;
    using Fsel.Identity.Infrastructure.Common;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class ConfirmOtpCommand : ConfirmOtpCommandModel, IRequest<MethodResult<UserOtpCode>>
    {
    }

    public class ConfirmOtpCommandHandler : IRequestHandler<ConfirmOtpCommand, MethodResult<UserOtpCode>>
    {
        private readonly UserOtpCodeHelper _userOtpCodeHelper;
        private readonly IUserOtpCodeRepository _userOtpCodeRepository;
        private readonly IMapper _mapper;

        public ConfirmOtpCommandHandler(UserOtpCodeHelper userOtpCodeHelper, IUserOtpCodeRepository userOtpCodeRepository, IMapper mapper)
        {
            _userOtpCodeHelper = userOtpCodeHelper;
            _userOtpCodeRepository = userOtpCodeRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<UserOtpCode>> Handle(ConfirmOtpCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<UserOtpCode> methodResult = new MethodResult<UserOtpCode>();
            var method = await _userOtpCodeHelper.ValidateOtp(request, cancellationToken);
            if (!method.IsOK)
            {
                methodResult.AddErrorBadRequest(method.ErrorMessages);
                return methodResult;
            }
            var userOtpCode = method.Result;
            if (userOtpCode != null)
            {
                userOtpCode.Status = EnumOtpCodeStatus.Verified;
                _userOtpCodeRepository.Update(userOtpCode);
                await _userOtpCodeRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }
            methodResult.Result = userOtpCode;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}