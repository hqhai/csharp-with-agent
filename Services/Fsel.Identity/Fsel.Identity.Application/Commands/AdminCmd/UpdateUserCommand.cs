// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.AdminCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Identity.Domain.Entities;
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
            var user = await _userManager.FindByIdAsync(request.Id.ToString());

            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(user));
                return methodResult;
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var role = userRoles.FirstOrDefault();
            if (userRoles.Contains(EnumRoleRegisterWithAdmin.Teacher.ToString()))
            {
                user = await _userManager.Users.Include(x => x.Human).ThenInclude(x => x!.Teacher).ThenInclude(x => x!.TeacherBankAccounts)
                                                        .FirstOrDefaultAsync(x => x.Id == request.Id.ToString(), cancellationToken);
                if (user == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(user));
                    return methodResult;
                }
                _mapper.Map(request, user.Human?.Teacher);
                if (request.TeacherBankAccount != null)
                {
                    _mapper.Map(request.TeacherBankAccount, user.Human?.Teacher?.TeacherBankAccounts?.FirstOrDefault(x => x.Status == EnumBankStatus.Approve));
                }
            }
            else if (role == EnumRoleRegisterWithAdmin.CSO.ToString())
            {
                user = await _userManager.Users.Include(x => x.Human).ThenInclude(x => x!.CSO).FirstOrDefaultAsync(x => x.Id == request.Id.ToString(), cancellationToken);
                if (user == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(user));
                    return methodResult;
                }
                _mapper.Map(request, user.Human?.CSO);
            }
            else if (role == EnumRoleRegisterWithAdmin.Moderator.ToString())
            {
                user = await _userManager.Users.Include(x => x.Human).FirstOrDefaultAsync(x => x.Id == request.Id.ToString(), cancellationToken);
                if (user == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(user));
                    return methodResult;
                }
            }

            _mapper.Map(request, user);
            _mapper.Map(request, user.Human);

            await _userManager.UpdateAsync(user);
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = _mapper.Map<UserModel>(user);
            return methodResult;
        }
    }
}
