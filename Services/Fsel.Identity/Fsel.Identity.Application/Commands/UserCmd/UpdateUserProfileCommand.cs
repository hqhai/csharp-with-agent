// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.UserCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
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
        private readonly IHumanRepository _humanRepository;

        public UpdateUserProfileCommandHandler(UserManager<User> userManager,
            IMapper mapper,
            AuthContext authContext,
            IHumanRepository humanRepository)
        {
            _userManager = userManager;
            _mapper = mapper;
            _authContext = authContext;
            _humanRepository = humanRepository;
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
                    methodResult.AddErrorBadRequest(nameof(EnumUserErrorCode.UserNotExist));
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
                    methodResult.AddErrorBadRequest(nameof(EnumUserErrorCode.UserNotExist));
                    return methodResult;
                }
                _mapper.Map(request, userView!.Human!.CSO);
            }
            else if (role == EnumRole.Student.ToString())
            {
                userView = await _userManager.Users.Include(x => x.Human)
                                                   .ThenInclude(x => x!.Student)
                                                   .ThenInclude(x => x!.ParentStudents)
                                                   .FirstOrDefaultAsync(x => x.Id == _authContext.CurrentUserId.ToString(), cancellationToken);
                if (userView == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumUserErrorCode.UserNotExist));
                    return methodResult;
                }
                var student = userView.Human!.Student!;
                if (request.Parent != null && student.CreatedByParent == false)
                {
                    if (student.ParentStudents == null || student.ParentStudents.Count == 0)
                    {
                        Human newHuman = _mapper.Map<Human>(request.Parent);
                        newHuman.Parent = _mapper.Map<Parent>(request.Parent);
                        newHuman.Parent.ParentStudents.Add(new ParentStudent
                        {
                            Student = student
                        });
                        _humanRepository.Add(newHuman);
                        await _humanRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        userView = await _userManager.Users.Include(x => x.Human)
                                                   .ThenInclude(x => x!.Student)
                                                   .ThenInclude(x => x!.ParentStudents)
                                                   .ThenInclude(x => x.Parent)
                                                   .ThenInclude(x => x!.Human)
                                                   .FirstOrDefaultAsync(x => x.Id == _authContext.CurrentUserId.ToString(), cancellationToken);
                        var human = userView!.Human!.Student!.ParentStudents.FirstOrDefault()!.Parent!.Human;
                        _mapper.Map(request.Parent, human);
                        _mapper.Map(request.Parent, human!.Parent);
                        _humanRepository.Update(human);
                        await _humanRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                    }
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
                        methodResult.AddErrorBadRequest(nameof(EnumUserErrorCode.UserNotExist));
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
                    methodResult.AddErrorBadRequest(nameof(EnumUserErrorCode.UserNotExist));
                    return methodResult;
                }
                _mapper.Map(request, userView!.Human!.Parent);
            }
            else if (role == EnumRoleRegisterWithAdmin.CSO.ToString())
            {
                userView = await _userManager.Users.Include(x => x.Human)
                                               .ThenInclude(x => x!.CSO)
                                               .FirstOrDefaultAsync(x => x.Id == _authContext.CurrentUserId.ToString(), cancellationToken);
                if (userView == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumUserErrorCode.UserNotExist));
                    return methodResult;
                }
                _mapper.Map(request, userView!.Human!.CSO);
            }
            else if (role == EnumRole.Moderator.ToString() || role == EnumRole.MasterAdmin.ToString() || role == EnumRole.Admin.ToString())
            {
                userView = await _userManager.Users.Include(x => x.Human)
                                    .FirstOrDefaultAsync(x => x.Id == _authContext.CurrentUserId.ToString(), cancellationToken);
                if (userView == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumUserErrorCode.UserNotExist));
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
