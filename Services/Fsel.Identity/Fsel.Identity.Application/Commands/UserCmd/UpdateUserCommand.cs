// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.UserCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.Models.CommandModels.Users;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;

    public class UpdateUserCommand : UpdateUserCommandModel, IRequest<MethodResult<UserModel>>
    {
    }

    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, MethodResult<UserModel>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;

        public UpdateUserCommandHandler(UserManager<User> userManager,
            IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<MethodResult<UserModel>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<UserModel>();

            User? user = new();
            if (request.Role == EnumRoleRegisterWithAdmin.Teacher)
            {
                user = await _userManager.Users.Include(x => x.Human).ThenInclude(x => x!.Teacher).FirstOrDefaultAsync(x => x.Id == request.Id.ToString(), cancellationToken);
                if (user == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumUserErrorCode.UserNotExist), nameof(request.Email), request.Email);
                    return methodResult;
                }
                _mapper.Map(request, user!.Human!.Teacher);
            }
            else if (request.Role == EnumRoleRegisterWithAdmin.CSO)
            {
                user = await _userManager.Users.Include(x => x.Human).ThenInclude(x => x!.CSO).FirstOrDefaultAsync(x => x.Email == request.Email, cancellationToken);
                if (user == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumUserErrorCode.UserNotExist), nameof(request.Email), request.Email);
                    return methodResult;
                }
                _mapper.Map(request, user!.Human!.CSO);
            }
            else if (request.Role == EnumRoleRegisterWithAdmin.Moderator)
            {
                user = await _userManager.Users.FirstOrDefaultAsync(x => x.Email == request.Email, cancellationToken);
                if (user == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumUserErrorCode.UserNotExist), nameof(request.Email), request.Email);
                    return methodResult;
                }
            }

            _mapper.Map(request, user);
            if (user.Human != null)
            {
                _mapper.Map(request, user.Human);
            }

            await _userManager.UpdateAsync(user);
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = _mapper.Map<UserModel>(user);
            return methodResult;
        }
    }
}
