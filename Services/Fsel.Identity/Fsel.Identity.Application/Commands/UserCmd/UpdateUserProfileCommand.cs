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
                userView = await UpdateTeacher(user.Id, request, cancellationToken).ConfigureAwait(false);
            }
            else if (role == EnumRole.CSO.ToString())
            {
                userView = await UpdateCSO(user.Id, request, cancellationToken).ConfigureAwait(false);
            }
            else if (role == EnumRole.Student.ToString())
            {
                userView = await UpdateStudent(user.Id, request, cancellationToken).ConfigureAwait(false);
            }
            else if (role == EnumRole.Parent.ToString())
            {
                userView = await UpdateParent(user.Id, request, cancellationToken).ConfigureAwait(false);
            }
            else if (role == EnumRole.Moderator.ToString() || role == EnumRole.MasterAdmin.ToString() || role == EnumRole.Admin.ToString())
            {
                userView = await UpdateRoleRemaining(user.Id, request, cancellationToken).ConfigureAwait(false);
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

        private async Task<User?> UpdateTeacher(Guid userId, UpdateUserProfileCommand request, CancellationToken cancellationToken)
        {
            var userView = await _userManager.Users.Include(x => x.Human)
                                                   .ThenInclude(x => x!.Teacher)
                                                   .ThenInclude(x => x!.TeacherBankAccounts)
                                                   .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
            if (userView == null)
            {
                return default;
            }
            var teacherBankAccounts = userView.Human?.Teacher?.TeacherBankAccounts;
            if (teacherBankAccounts != null && teacherBankAccounts.Any())
            {
                var teacherBankAccountNew = teacherBankAccounts.FirstOrDefault(x => x.Status == EnumBankStatus.New);
                if (teacherBankAccountNew != null)
                {
                    return default;
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
            await _userManager.UpdateAsync(userView);
            return userView;
        }

        private async Task<User?> UpdateCSO(Guid userId, UpdateUserProfileCommand request, CancellationToken cancellationToken)
        {
            var userView = await _userManager.Users.Include(x => x.Human)
                                                  .ThenInclude(x => x!.CSO)
                                                  .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
            if (userView == null)
            {
                return default;
            }
            _mapper.Map(request, userView.Human?.CSO);
            _mapper.Map(request, userView);
            _mapper.Map(request, userView.Human);
            await _userManager.UpdateAsync(userView);
            return userView;
        }

        private async Task<User?> UpdateStudent(Guid userId, UpdateUserProfileCommand request, CancellationToken cancellationToken)
        {
            var userView = await _userManager.Users.Include(x => x.Human)
                                                   .ThenInclude(x => x!.Student)
                                                   .ThenInclude(x => x!.ParentStudents)
                                                   .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
            if (userView == null)
            {
                return default;
            }
            var student = userView.Human?.Student;
            if (request.Parent != null && student != null && student.CreatedByParent == false)
            {
                if (student.ParentStudents == null || student.ParentStudents.Count == 0)
                {
                    if (string.IsNullOrEmpty(request.Parent.FullName))
                    {
                        return default;
                    }

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
                                               .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
                    var human = userView?.Human?.Student?.ParentStudents.FirstOrDefault()?.Parent?.Human;
                    if (human != null)
                    {
                        _mapper.Map(request.Parent, human);
                        _mapper.Map(request.Parent, human.Parent);
                        _humanRepository.Update(human);
                        await _humanRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                    }
                }
            }
            if (userView == null)
            {
                return default;
            }
            _mapper.Map(request, userView.Human?.Student);
            _mapper.Map(request, userView);
            _mapper.Map(request, userView.Human);
            await _userManager.UpdateAsync(userView);
            return userView;
        }

        private async Task<User?> UpdateParent(Guid userId, UpdateUserProfileCommand request, CancellationToken cancellationToken)
        {
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
                    return default;
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
                        users.Add(userStudent);
                    }
                }
            }
            else
            {
                userView = await _userManager.Users.Include(x => x.Human)
                                             .ThenInclude(x => x!.Parent)
                                             .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
            }

            if (userView == null)
            {
                return default;
            }
            _mapper.Map(request, userView.Human?.Parent);
            _mapper.Map(request, userView);
            _mapper.Map(request, userView.Human);
            await _userManager.UpdateAsync(userView);
            foreach (var item in users)
            {
                await _userManager.UpdateAsync(item);
            }
            return userView;
        }

        private async Task<User?> UpdateRoleRemaining(Guid userId, UpdateUserProfileCommand request, CancellationToken cancellationToken)
        {
            var userView = await _userManager.Users.Include(x => x.Human)
                                     .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
            if (userView == null)
            {
                return default;
            }
            _mapper.Map(request, userView);
            _mapper.Map(request, userView.Human);
            await _userManager.UpdateAsync(userView);
            return userView;
        }
    }
}
