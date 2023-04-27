// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.UserCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.Models.CommandModels.Users;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;

    public class UpdateUserProfileCommand : UpdateUserProfileCommandModel, IRequest<MethodResult<UserModel>>
    {
    }

    public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, MethodResult<UserModel>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;

        public UpdateUserProfileCommandHandler(UserManager<User> userManager,
            IMapper mapper,
            AuthContext authContext)
        {
            _userManager = userManager;
            _mapper = mapper;
            _authContext = authContext;
        }

        public async Task<MethodResult<UserModel>> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<UserModel>();
            var user = await _userManager.FindByIdAsync(_authContext.CurrentUserId.ToString());

            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumUserErrorCode.UserNotExist));
                return methodResult;
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            User? userView = null;
            if (userRoles.FirstOrDefault() == EnumRole.Teacher.ToString())
            {
                userView = await _userManager.Users.Include(x => x.Human)
                                                   .ThenInclude(x => x!.Teacher)
                                                   .FirstOrDefaultAsync(x => x.Id == _authContext.CurrentUserId.ToString(), cancellationToken);
                if (userView == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumUserErrorCode.UserNotExist), nameof(request.Email), request.Email);
                    return methodResult;
                }
                _mapper.Map(request, userView!.Human!.Teacher);
            }
            else if (userRoles.FirstOrDefault() == EnumRole.CSO.ToString())
            {
                userView = await _userManager.Users.Include(x => x.Human)
                                                   .ThenInclude(x => x!.CSO)
                                                   .FirstOrDefaultAsync(x => x.Id == _authContext.CurrentUserId.ToString(), cancellationToken);
                if (userView == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumUserErrorCode.UserNotExist), nameof(request.Email), request.Email);
                    return methodResult;
                }
                _mapper.Map(request, userView!.Human!.CSO);
            }
            else if (userRoles.FirstOrDefault() == EnumRole.Student.ToString())
            {
                userView = await _userManager.Users.Include(x => x.Human)
                                                   .ThenInclude(x => x!.Student)
                                                   .FirstOrDefaultAsync(x => x.Id == _authContext.CurrentUserId.ToString(), cancellationToken);
                if (userView == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumUserErrorCode.UserNotExist), nameof(request.Email), request.Email);
                    return methodResult;
                }
                _mapper.Map(request, userView!.Human!.Student);
            }
            else if (userRoles.FirstOrDefault() == EnumRole.Parent.ToString())
            {
                if (request.StudentId != null)
                {
                    userView = await _userManager.Users.Include(x => x.Human)
                                                  .ThenInclude(x => x!.Parent)
                                                  .ThenInclude(x => x!.ParentStudents.Where(y => !y.IsDeleted && y.Student != null && y.StudentId == request.StudentId))
                                                  .ThenInclude(x => x.Student)
                                                  .FirstOrDefaultAsync(x => x.Id == _authContext.CurrentUserId.ToString(), cancellationToken);

                    _mapper.Map(request, userView!.Human!.Parent!.ParentStudents.FirstOrDefault()!.Student);
                }
                else
                {
                    userView = await _userManager.Users.Include(x => x.Human)
                                                 .ThenInclude(x => x!.Parent)
                                                 .FirstOrDefaultAsync(x => x.Id == _authContext.CurrentUserId.ToString(), cancellationToken);
                }

                if (userView == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumUserErrorCode.UserNotExist), nameof(request.Email), request.Email);
                    return methodResult;
                }
                _mapper.Map(request, userView!.Human!.Parent);
            }
            else if (userRoles.FirstOrDefault() == EnumRoleRegisterWithAdmin.CSO.ToString())
            {
                userView = await _userManager.Users.Include(x => x.Human)
                                               .ThenInclude(x => x!.CSO)
                                               .FirstOrDefaultAsync(x => x.Email == request.Email, cancellationToken);
                if (userView == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumUserErrorCode.UserNotExist), nameof(request.Email), request.Email);
                    return methodResult;
                }
                _mapper.Map(request, userView!.Human!.CSO);
            }
            else if (userRoles.FirstOrDefault() == EnumRole.Moderator.ToString() || userRoles.FirstOrDefault() == EnumRole.MasterAdmin.ToString() || userRoles.FirstOrDefault() == EnumRole.Admin.ToString())
            {
                userView = await _userManager.Users.FirstOrDefaultAsync(x => x.Email == request.Email, cancellationToken);
                if (userView == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumUserErrorCode.UserNotExist), nameof(request.Email), request.Email);
                    return methodResult;
                }
            }

            _mapper.Map(request, userView);
            _mapper.Map(request, userView!.Human);

            await _userManager.UpdateAsync(userView);

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = _mapper.Map<UserModel>(user);
            return methodResult;
        }
    }
}
