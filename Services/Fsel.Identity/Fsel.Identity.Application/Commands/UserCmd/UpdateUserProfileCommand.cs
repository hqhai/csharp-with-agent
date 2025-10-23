// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.UserCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.Users;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
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
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(user));
                return methodResult;
            }
            if (request.PhoneNumber != null && user.UserName == user.PhoneNumber)
            {
                var checkUserName = await _userManager.Users.AnyAsync(x => x.Id != user.Id && x.UserName == request.PhoneNumber, cancellationToken);

                if (checkUserName)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(request.PhoneNumber));
                    return methodResult;
                }
            }
            var userRoles = await _userManager.GetRolesAsync(user);
            var role = userRoles.FirstOrDefault();
            User? userView = null;
            if (role == EnumRole.Teacher.ToString())
            {
                var method = await UpdateTeacherAsync(user.Id, request, cancellationToken).ConfigureAwait(false);
                if (!method.IsOK)
                {
                    methodResult.AddErrorBadRequest(method.ErrorMessages);
                    return methodResult;
                }
                userView = method.Result;
            }
            else if (role == EnumRole.CSO.ToString())
            {
                var method = await UpdateCSOAsync(user.Id, request, cancellationToken).ConfigureAwait(false);
                if (!method.IsOK)
                {
                    methodResult.AddErrorBadRequest(method.ErrorMessages);
                    return methodResult;
                }
                userView = method.Result;
            }
            else if (role == EnumRole.Student.ToString() || role == EnumRole.StudentCampus.ToString())
            {
                var method = await UpdateStudentAsync(user.Id, request, cancellationToken).ConfigureAwait(false);
                if (!method.IsOK)
                {
                    methodResult.AddErrorBadRequest(method.ErrorMessages);
                    return methodResult;
                }
                userView = method.Result;
            }
            else if (role == EnumRole.Parent.ToString())
            {
                var method = await UpdateParentAsync(user.Id, request, cancellationToken).ConfigureAwait(false);
                if (!method.IsOK)
                {
                    methodResult.AddErrorBadRequest(method.ErrorMessages);
                    return methodResult;
                }
                userView = method.Result;
            }
            else if (role == EnumRole.Moderator.ToString() || role == EnumRole.MasterAdmin.ToString() || role == EnumRole.Admin.ToString())
            {
                var method = await UpdateRoleRemainingAsync(user.Id, request, cancellationToken).ConfigureAwait(false);
                if (!method.IsOK)
                {
                    methodResult.AddErrorBadRequest(method.ErrorMessages);
                    return methodResult;
                }
                userView = method.Result;
            }
            if (userView == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(userView));
                return methodResult;
            }
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = _mapper.Map<UserModel>(userView);
            return methodResult;
        }

        private async Task<MethodResult<User>> UpdateTeacherAsync(Guid userId, UpdateUserProfileCommand request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<User>();
            var userView = await _userManager.Users.Include(x => x.Human)
                                                   .ThenInclude(x => x!.Teacher)
                                                   .ThenInclude(x => x!.TeacherBankAccounts)
                                                   .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
            if (userView == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(User), userId);
                return methodResult;
            }
            var teacherBankAccounts = userView.Human?.Teacher?.TeacherBankAccounts;
            if (teacherBankAccounts != null && teacherBankAccounts.Any())
            {
                var teacherBankAccountNew = teacherBankAccounts.FirstOrDefault(x => x.Status == EnumBankStatus.New);
                if (teacherBankAccountNew != null)
                {
                    return methodResult;
                }
                if (request.TeacherBankAccount != null)
                {
                    var teacherBankAccount = _mapper.Map<TeacherBankAccount>(request.TeacherBankAccount);
                    teacherBankAccount.Status = EnumBankStatus.New;
                    userView.Human?.Teacher?.TeacherBankAccounts?.Add(teacherBankAccount);
                }
            }
            _mapper.Map(request, userView.Human?.Teacher);
            _mapper.Map(request, userView);
            _mapper.Map(request, userView.Human);
            if (!userView.IsValid())
            {
                methodResult.AddErrorBadRequest(userView.ErrorMessages);
                return methodResult;
            }
            if (userView.Human != null && !userView.Human.IsValid())
            {
                methodResult.AddErrorBadRequest(userView.Human.ErrorMessages);
                return methodResult;
            }

            if (userView.Human?.Teacher != null && !userView.Human.Teacher.IsValid())
            {
                methodResult.AddErrorBadRequest(userView.Human.Teacher.ErrorMessages);
                return methodResult;
            }

            await _userManager.UpdateAsync(userView);

            methodResult.Result = userView;
            return methodResult;
        }

        private async Task<MethodResult<User>> UpdateCSOAsync(Guid userId, UpdateUserProfileCommand request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<User>();
            var userView = await _userManager.Users.Include(x => x.Human)
                                                  .ThenInclude(x => x!.CSO)
                                                  .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
            if (userView == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(User), userId);
                return methodResult;
            }
            _mapper.Map(request, userView.Human?.CSO);
            _mapper.Map(request, userView);
            _mapper.Map(request, userView.Human);
            if (!userView.IsValid())
            {
                methodResult.AddErrorBadRequest(userView.ErrorMessages);
                return methodResult;
            }
            if (userView.Human != null && !userView.Human.IsValid())
            {
                methodResult.AddErrorBadRequest(userView.Human.ErrorMessages);
                return methodResult;
            }

            if (userView.Human?.CSO != null && !userView.Human.CSO.IsValid())
            {
                methodResult.AddErrorBadRequest(userView.Human.CSO.ErrorMessages);
                return methodResult;
            }

            await _userManager.UpdateAsync(userView);
            methodResult.Result = userView;
            return methodResult;
        }

        private async Task<MethodResult<User>> UpdateStudentAsync(Guid userId, UpdateUserProfileCommand request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<User>();
            var userView = await _userManager.Users.Include(x => x.Human)
                                                   .ThenInclude(x => x!.Student)
                                                   .ThenInclude(x => x!.ParentStudents)
                                                   .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
            if (userView == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(User), userId);
                return methodResult;
            }
            var student = userView.Human?.Student;
            if (student != null)
            {
                student.ParentEmail = string.IsNullOrEmpty(request.Parent?.Email) ? null : request.Parent?.Email;
                student.ParentPhoneNumber = string.IsNullOrEmpty(request.Parent?.PhoneNumber) ? null : request.Parent?.PhoneNumber;
            }
            if (request.Parent != null && student != null && !student.CreatedByParent)
            {
                if (student.ParentStudents == null || student.ParentStudents.Count == 0)
                {
                    if (string.IsNullOrEmpty(request.Parent.FullName))
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(User), request.Parent.FullName);
                        return methodResult;
                    }

                    Human newHuman = _mapper.Map<Human>(request.Parent);
                    newHuman.Parent = _mapper.Map<Parent>(request.Parent);
                    newHuman.Parent.ParentStudents.Add(new ParentStudent
                    {
                        Student = student
                    });

                    if (!newHuman.IsValid())
                    {
                        methodResult.AddErrorBadRequest(userView.ErrorMessages);
                        return methodResult;
                    }
                    if (newHuman.Parent != null && !newHuman.Parent.IsValid())
                    {
                        methodResult.AddErrorBadRequest(newHuman.Parent.ErrorMessages);
                        return methodResult;
                    }

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
                                               .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
                    if (userView == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(User), userId);
                        return methodResult;
                    }
                    var human = userView.Human?.Student?.ParentStudents.FirstOrDefault()?.Parent?.Human;
                    if (human != null)
                    {
                        _mapper.Map(request.Parent, human);
                        _mapper.Map(request.Parent, human.Parent);

                        if (!human.IsValid())
                        {
                            methodResult.AddErrorBadRequest(userView.ErrorMessages);
                            return methodResult;
                        }
                        if (human.Parent != null && !human.Parent.IsValid())
                        {
                            methodResult.AddErrorBadRequest(human.Parent.ErrorMessages);
                            return methodResult;
                        }

                        _humanRepository.Update(human);
                        await _humanRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                    }
                }
            }
            _mapper.Map(request, userView.Human?.Student);
            _mapper.Map(request, userView);
            _mapper.Map(request, userView.Human);
            if (!userView.IsValid())
            {
                methodResult.AddErrorBadRequest(userView.ErrorMessages);
                return methodResult;
            }
            if (userView.Human != null && !userView.Human.IsValid())
            {
                methodResult.AddErrorBadRequest(userView.Human.ErrorMessages);
                return methodResult;
            }

            if (userView.Human?.Student != null && !userView.Human.Student.IsValid())
            {
                methodResult.AddErrorBadRequest(userView.Human.Student.ErrorMessages);
                return methodResult;
            }

            await _userManager.UpdateAsync(userView);
            methodResult.Result = userView;
            return methodResult;
        }

        private async Task<MethodResult<User>> UpdateParentAsync(Guid userId, UpdateUserProfileCommand request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<User>();
            List<User> users = new List<User>();
            User? userView = null;
            if (request.Students != null && request.Students.Any())
            {
                userView = await _userManager.Users.Include(x => x.Human)
                                             .ThenInclude(x => x!.Parent)
                                             .ThenInclude(x => x!.ParentStudents.Where(y => !y.IsDeleted && y.Student != null))
                                             .ThenInclude(x => x.Student)
                                             .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
                if (userView == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(User), userId);
                    return methodResult;
                }

                foreach (var item in request.Students)
                {
                    var userStudent = await _userManager.Users.Include(x => x.Human)
                                              .ThenInclude(x => x!.Student).FirstOrDefaultAsync(x => x.Human!.Student!.Id == item.StudentId, cancellationToken);
                    if (userStudent != null)
                    {
                        _mapper.Map(item, userStudent);
                        _mapper.Map(item, userStudent.Human);
                        _mapper.Map(item, userStudent.Human?.Student);
                        if (!userStudent.IsValid())
                        {
                            methodResult.AddErrorBadRequest(userView.ErrorMessages);
                            return methodResult;
                        }
                        if (userStudent.Human != null && !userStudent.Human.IsValid())
                        {
                            methodResult.AddErrorBadRequest(userStudent.Human.ErrorMessages);
                            return methodResult;
                        }

                        if (userStudent.Human?.Student != null && !userStudent.Human.Student.IsValid())
                        {
                            methodResult.AddErrorBadRequest(userStudent.Human.Student.ErrorMessages);
                            return methodResult;
                        }
                        users.Add(userStudent);
                    }
                }
            }
            else
            {
                userView = await _userManager.Users.Include(x => x.Human)
                                             .ThenInclude(x => x!.Parent)
                                             .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
                if (userView == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(User), userId);
                    return methodResult;
                }
            }

            _mapper.Map(request, userView.Human?.Parent);
            _mapper.Map(request, userView);
            _mapper.Map(request, userView.Human);
            if (!userView.IsValid())
            {
                methodResult.AddErrorBadRequest(userView.ErrorMessages);
                return methodResult;
            }
            if (userView.Human != null && !userView.Human.IsValid())
            {
                methodResult.AddErrorBadRequest(userView.Human.ErrorMessages);
                return methodResult;
            }

            if (userView.Human?.Parent != null && !userView.Human.Parent.IsValid())
            {
                methodResult.AddErrorBadRequest(userView.Human.Parent.ErrorMessages);
                return methodResult;
            }

            await _userManager.UpdateAsync(userView);
            foreach (var item in users)
            {
                await _userManager.UpdateAsync(item);
            }
            methodResult.Result = userView;
            return methodResult;
        }

        private async Task<MethodResult<User>> UpdateRoleRemainingAsync(Guid userId, UpdateUserProfileCommand request, CancellationToken cancellationToken)
        {
            MethodResult<User> methodResult = new MethodResult<User>();
            var userView = await _userManager.Users.Include(x => x.Human)
                                     .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
            if (userView == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(User), userId);
                return methodResult;
            }
            _mapper.Map(request, userView);
            _mapper.Map(request, userView.Human);
            if (!userView.IsValid())
            {
                methodResult.AddErrorBadRequest(userView.ErrorMessages);
                return methodResult;
            }
            if (userView.Human != null && !userView.Human.IsValid())
            {
                methodResult.AddErrorBadRequest(userView.Human.ErrorMessages);
                return methodResult;
            }

            await _userManager.UpdateAsync(userView);

            methodResult.Result = userView;
            return methodResult;
        }
    }
}
