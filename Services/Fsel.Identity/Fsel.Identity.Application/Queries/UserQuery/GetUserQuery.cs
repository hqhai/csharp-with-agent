// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.UserQuery
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Application.Services.SystemService;
    using Fsel.Identity.Application.Services.TrainingService;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetUserQuery : IRequest<MethodResult<UserProfileModel>>
    {
        public Guid UserId { get; set; }
    }

    public class GetUserQueryHandler : IRequestHandler<GetUserQuery, MethodResult<UserProfileModel>>
    {
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly ITrainingService _trainingService;
        private readonly ISystemService _systemService;

        public GetUserQueryHandler(IMapper mapper, UserManager<User> userManager, ITrainingService trainingService, ISystemService systemService)
        {
            _mapper = mapper;
            _userManager = userManager;
            _trainingService = trainingService;
            _systemService = systemService;
        }

        public async Task<MethodResult<UserProfileModel>> Handle(GetUserQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<UserProfileModel> methodResult = new MethodResult<UserProfileModel>();

            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(user));
                return methodResult;
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            User? userView = null;
            if (userRoles.FirstOrDefault() == EnumRole.CSO.ToString())
            {
                userView = await _userManager.Users.Include(x => x.Human)
                                                   .ThenInclude(x => x!.CSO)
                                                   .FirstOrDefaultAsync(x => x.Id == request.UserId, cancellationToken);
            }
            else if (userRoles.FirstOrDefault() == EnumRole.Teacher.ToString())
            {
                userView = await _userManager.Users.Include(x => x.Human)
                                                   .ThenInclude(x => x!.Teacher)
                                                   .ThenInclude(x => x!.TeacherBankAccounts)
                                                   .FirstOrDefaultAsync(x => x.Id == request.UserId, cancellationToken);
            }
            else if (userRoles.FirstOrDefault() == EnumRole.Student.ToString() || userRoles.FirstOrDefault() == EnumRole.StudentCampus.ToString())
            {
                userView = await _userManager.Users.Include(x => x.Human)
                                                   .ThenInclude(x => x!.Student)
                                                   .FirstOrDefaultAsync(x => x.Id == request.UserId && x.EmailConfirmed, cancellationToken);
                if (userView?.Human?.Student?.CreatedByParent == false)
                {
                    userView = await _userManager.Users.Include(x => x.Human)
                                                  .ThenInclude(x => x!.Student)
                                                  .ThenInclude(x => x!.ParentStudents)
                                                  .ThenInclude(x => x!.Parent)
                                                  .ThenInclude(x => x!.Human)
                                                  .FirstOrDefaultAsync(x => x.Id == request.UserId, cancellationToken);
                }
            }
            else if (userRoles.FirstOrDefault() == EnumRole.Parent.ToString())
            {
                userView = await _userManager.Users.Include(x => x.Human)
                                                   .ThenInclude(x => x!.Parent)
                                                   .ThenInclude(x => x!.ParentStudents.Where(y => !y.IsDeleted))
                                                   .ThenInclude(x => x.Student)
                                                   .FirstOrDefaultAsync(x => x.Id == request.UserId && x.EmailConfirmed, cancellationToken);
            }
            else if (userRoles.FirstOrDefault() == EnumRole.AdminSchool.ToString())
            {
                userView = await _userManager.Users.Include(x => x.Human)
                                                   .Include(x => x.UserSchools)
                                                   .FirstOrDefaultAsync(x => x.Id == request.UserId && x.EmailConfirmed, cancellationToken);
            }
            else if (userRoles.FirstOrDefault() == EnumRole.Moderator.ToString() || userRoles.FirstOrDefault() == EnumRole.MasterAdmin.ToString() || userRoles.FirstOrDefault() == EnumRole.Admin.ToString())
            {
                userView = await _userManager.Users.Include(x => x.Human)
                                                 .FirstOrDefaultAsync(x => x.Id == request.UserId, cancellationToken);
            }
            var userModel = _mapper.Map<UserProfileModel>(userView ?? user);
            if (userView != null)
            {
                _mapper.Map(userView.Human, userModel);

                if ((userRoles.FirstOrDefault() == EnumRole.Student.ToString() || userRoles.FirstOrDefault() == EnumRole.StudentCampus.ToString()) && userView.Human?.Student?.CreatedByParent == false && userView.Human?.Student?.ParentStudents.Count > 0)
                {
                    var classStudent = await _trainingService.GetClassToStudentId(userView!.Human!.Student.Id);
                    userModel!.Parent = _mapper.Map<ParentProfileModel>(userView!.Human!.Student!.ParentStudents!.FirstOrDefault()!.Parent);
                    _mapper.Map(userView!.Human!.Student, userModel);
                    if (classStudent.Content?.Result != null)
                    {
                        userModel.CodeClass = classStudent!.Content!.Result!.Code;
                    }
                }

                if (userRoles.FirstOrDefault() == EnumRole.Parent.ToString() && userView.Human?.Parent?.ParentStudents != null && userView.Human?.Parent?.ParentStudents.Count > 0)
                {
                    userModel!.Students = _mapper.Map<List<StudentProfileModel>>(userView!.Human!.Parent!.ParentStudents.Select(x => x.Student).ToList());
                    _mapper.Map(userView!.Human!.Parent, userModel);

                    foreach (var student in userModel!.Students)
                    {
                        var classStudent = await _trainingService.GetClassToStudentId(student!.Id);
                        if (classStudent.Content?.Result != null)
                        {
                            student.CodeClass = classStudent!.Content!.Result!.Code;
                        }
                        var userName = userView!.Human!.Parent!.ParentStudents.Select(x => x.Student).FirstOrDefault(x => x!.Id == student.Id)!.Human!.User!.UserName;
                        student.UserName = userName;
                    }
                }

                if (userRoles.FirstOrDefault() == EnumRole.Teacher.ToString() && userView.Human?.Teacher != null)
                {
                    userModel!.TeacherBankAccounts = _mapper.Map<IList<TeacherBankAccountModel>>(userView!.Human!.Teacher!.TeacherBankAccounts?.Where(x => x.Status == EnumBankStatus.Approve).ToList());
                    _mapper.Map(userView!.Human!.Teacher, userModel);
                }

                if (userRoles.FirstOrDefault() == EnumRole.CSO.ToString())
                {
                    _mapper.Map(userView!.Human!.CSO, userModel);
                }
                if (userRoles.FirstOrDefault() == EnumRole.AdminSchool.ToString())
                {
                    var schoolId = user.UserSchools.FirstOrDefault()?.SchoolId;
                    if (schoolId.HasValue)
                    {
                        var schoolResult = await _systemService.GetSchoolByIds(new List<Guid> { schoolId.Value });
                        userModel.School = schoolResult.Content?.Result?.FirstOrDefault()?.Name;
                        userModel.SchoolName = schoolResult.Content?.Result?.FirstOrDefault()?.Name;
                    }
                }
                userModel.Roles = userRoles;
            }

            methodResult.Result = userModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
