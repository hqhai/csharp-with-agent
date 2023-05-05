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

            var userEmail = await _userManager.Users.FirstOrDefaultAsync(x => x.Id != _authContext.CurrentUserId.ToString() && x.Email == request.Email, cancellationToken);
            if (userEmail != null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumUserErrorCode.EmailAlreadyExists));
                return methodResult;
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var role = userRoles.FirstOrDefault();
            List<User> users = new List<User>();
            User? userView = null;
            if (role == EnumRole.Teacher.ToString())
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
            else if (role == EnumRole.CSO.ToString())
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
            else if (role == EnumRole.Student.ToString())
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
            else if (role == EnumRole.Parent.ToString())
            {
                if (request.Students != null && request.Students.Count > 0)
                {
                    userView = await _userManager.Users.Include(x => x.Human)
                                                  .ThenInclude(x => x!.Parent)
                                                  .ThenInclude(x => x!.ParentStudents.Where(y => !y.IsDeleted && y.Student != null))
                                                  .ThenInclude(x => x.Student)
                                                  .FirstOrDefaultAsync(x => x.Id == _authContext.CurrentUserId.ToString(), cancellationToken);
                    if (userView == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumUserErrorCode.UserNotExist), nameof(request.Email), request.Email);
                        return methodResult;
                    }

                    foreach (var item in request.Students)
                    {
                        var userStudent = await _userManager.Users.Include(x => x.Human)
                                                  .ThenInclude(x => x!.Student).FirstOrDefaultAsync(x => x.Human!.Student!.Id == item.StudentId, cancellationToken);
                        _mapper.Map(item, userStudent);
                        _mapper.Map(item, userStudent!.Human);
                        _mapper.Map(item, userStudent!.Human!.Student);
                        users.Add(userStudent);
                    }
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
            else if (role == EnumRoleRegisterWithAdmin.CSO.ToString())
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
            else if (role == EnumRole.Moderator.ToString() || role == EnumRole.MasterAdmin.ToString() || role == EnumRole.Admin.ToString())
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

            foreach (var item in users)
            {
                await _userManager.UpdateAsync(item);
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = _mapper.Map<UserModel>(user);
            return methodResult;
        }
    }
}
