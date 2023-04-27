// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.UserCmd
{
    using System;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;

    public class UpdatePhoneNumberUserCommand : IRequest<MethodResult<UserModel>>
    {
        public string? PhoneNumber { get; set; }
    }

    public class UpdatePhoneNumberUserCommandHandler : IRequestHandler<UpdatePhoneNumberUserCommand, MethodResult<UserModel>>
    {
        private readonly UserManager<User> _userManager;
        private readonly AuthContext _authContext;
        private readonly IMapper _mapper;

        public UpdatePhoneNumberUserCommandHandler(UserManager<User> userManager, AuthContext authContext,
            IMapper mapper)
        {
            _userManager = userManager;
            _authContext = authContext;
            _mapper = mapper;
        }

        public async Task<MethodResult<UserModel>> Handle(UpdatePhoneNumberUserCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<UserModel>();
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == _authContext.CurrentUserId.ToString(), cancellationToken);
            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumUserErrorCode.UserNotExist));
                return methodResult;
            }

            user.PhoneNumber = request.PhoneNumber;
            await _userManager.UpdateAsync(user);

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = _mapper.Map<UserModel>(user);
            return methodResult;
        }
    }
}
