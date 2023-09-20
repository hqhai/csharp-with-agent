// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.UserQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Identity.Application.Services.LmsCourseService;
    using Fsel.Identity.Application.Services.OrderService;
    using Fsel.Identity.Application.Services.TrainingService;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;

    public class GetUserProfileQuery : IRequest<MethodResult<UserProfileModel>>
    {
    }

    public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, MethodResult<UserProfileModel>>
    {
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;
        private readonly UserManager<User> _userManager;
        private readonly ITrainingService _trainingService;
        private readonly IOrderService _orderService;
        private readonly ILmsCourseService _lmsCourseService;

        public GetUserProfileQueryHandler(IMapper mapper, AuthContext authContext, UserManager<User> userManager, ITrainingService trainingService, IOrderService orderService, ILmsCourseService lmsCourseService)
        {
            _mapper = mapper;
            _authContext = authContext;
            _userManager = userManager;
            _trainingService = trainingService;
            _orderService = orderService;
            _lmsCourseService = lmsCourseService;
        }

        public async Task<MethodResult<UserProfileModel>> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<UserProfileModel> methodResult = new MethodResult<UserProfileModel>();

            var user = await _userManager.FindByIdAsync(_authContext.CurrentUserId.ToString());

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
                                                   .FirstOrDefaultAsync(x => x.Id == _authContext.CurrentUserId.ToString(), cancellationToken);
            }
            else if (userRoles.FirstOrDefault() == EnumRole.Teacher.ToString())
            {
                userView = await _userManager.Users.Include(x => x.Human)
                                                   .ThenInclude(x => x!.Teacher)
                                                   .ThenInclude(x => x!.TeacherBankAccounts)
                                                   .FirstOrDefaultAsync(x => x.Id == _authContext.CurrentUserId.ToString(), cancellationToken);
            }
            else if (userRoles.FirstOrDefault() == EnumRole.Student.ToString())
            {
                userView = await _userManager.Users.Include(x => x.Human)
                                                   .ThenInclude(x => x!.Student)
                                                   .ThenInclude(x => x!.ParentStudents)
                                                   .FirstOrDefaultAsync(x => x.Id == _authContext.CurrentUserId.ToString(), cancellationToken);
                var student = userView?.Human?.Student;
                if (student != null && student.ParentStudents != null && student.ParentStudents.Count > 0)
                {
                    userView = await _userManager.Users.Include(x => x.Human)
                                                  .ThenInclude(x => x!.Student)
                                                  .ThenInclude(x => x!.ParentStudents)
                                                  .ThenInclude(x => x!.Parent)
                                                  .ThenInclude(x => x!.Human)
                                                  .FirstOrDefaultAsync(x => x.Id == _authContext.CurrentUserId.ToString(), cancellationToken);
                }
            }
            else if (userRoles.FirstOrDefault() == EnumRole.Parent.ToString())
            {
                userView = await _userManager.Users.Include(x => x.Human)
                                                   .ThenInclude(x => x!.Parent)
                                                   .ThenInclude(x => x!.ParentStudents)
                                                   .FirstOrDefaultAsync(x => x.Id == _authContext.CurrentUserId.ToString(), cancellationToken);
                var parentStudents = userView?.Human?.Parent?.ParentStudents;
                if (parentStudents != null && parentStudents.Count > 0)
                {
                    userView = await _userManager.Users.Include(x => x.Human)
                                                                      .ThenInclude(x => x!.Parent)
                                                                      .ThenInclude(x => x!.ParentStudents)
                                                                      .ThenInclude(x => x.Student)
                                                                      .ThenInclude(x => x!.Human)
                                                                      .ThenInclude(x => x!.User)
                                                                      .FirstOrDefaultAsync(x => x.Id == _authContext.CurrentUserId.ToString(), cancellationToken);
                }
            }
            else if (userRoles.FirstOrDefault() == EnumRole.Moderator.ToString() || userRoles.FirstOrDefault() == EnumRole.MasterAdmin.ToString() || userRoles.FirstOrDefault() == EnumRole.Admin.ToString())
            {
                userView = await _userManager.Users.Include(x => x.Human).FirstOrDefaultAsync(x => x.Id == _authContext.CurrentUserId.ToString(), cancellationToken);
            }

            var userModel = _mapper.Map<UserProfileModel>(userView ?? user);
            _mapper.Map(userView?.Human, userModel);

            if (userRoles.FirstOrDefault() == EnumRole.Student.ToString())
            {
                var student = userView?.Human?.Student;
                if (student != null)
                {
                    var countResult = await _lmsCourseService.CountResultByStudentId(student.Id);
                    if (!countResult.IsSuccessStatusCode)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallCourseServiceError), nameof(countResult));
                        return methodResult;
                    }
                    userModel.CountPTResult = countResult?.Content?.Result ?? default;
                    if (student.CreatedByParent == false)
                    {
                        var classStudent = await _trainingService.GetClassByStudentId(student.Id);
                        if (!classStudent.IsSuccessStatusCode)
                        {
                            methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallTrainingServiceError), nameof(classStudent));
                            return methodResult;
                        }
                        _mapper.Map(student, userModel);
                        userModel.CodeClass = classStudent.Content?.Result?.Code;
                        if (student.ParentStudents.Count > 0)
                        {
                            var parent = student.ParentStudents.FirstOrDefault()?.Parent;
                            if (parent != null)
                            {
                                userModel.Parent = _mapper.Map<ParentProfileModel>(parent.Human);
                                userModel.Parent.Occupation = parent.Occupation;
                            }
                        }
                        var package = await _orderService.GetPackages();
                        if (!package.IsSuccessStatusCode)
                        {
                            methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallOrderServiceError), nameof(package));
                            return methodResult;
                        }
                        userModel.Membership = package.Content?.Result?.FirstOrDefault(p => p.Id == userModel.PackageId)?.Code;
                    }
                }
            }

            if (userRoles.FirstOrDefault() == EnumRole.Parent.ToString())
            {
                var parent = userView?.Human?.Parent;
                if (parent != null && parent.ParentStudents != null && parent.ParentStudents.Count > 0)
                {
                    userModel.Students = _mapper.Map<List<StudentProfileModel>>(parent.ParentStudents.Select(x => x.Student).ToList());
                    _mapper.Map(parent, userModel);

                    foreach (var student in userModel.Students)
                    {
                        var classStudent = await _trainingService.GetClassByStudentId(student.Id);
                        if (!classStudent.IsSuccessStatusCode)
                        {
                            methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallTrainingServiceError), nameof(classStudent));
                            return methodResult;
                        }
                        if (classStudent.Content?.Result != null)
                        {
                            student.CodeClass = classStudent!.Content!.Result!.Code;
                        }
                        var userName = parent.ParentStudents.Select(x => x.Student).FirstOrDefault(x => x!.Id == student.Id)?.Human?.User?.UserName;
                        student.UserName = userName;
                    }
                }
            }

            if (userRoles.FirstOrDefault() == EnumRole.Teacher.ToString())
            {
                var teacher = userView?.Human?.Teacher;
                if (teacher != null)
                {
                    userModel.TeacherBankAccounts = _mapper.Map<IList<TeacherBankAccountModel>>(teacher.TeacherBankAccounts);
                    _mapper.Map(teacher, userModel);
                }
            }

            if (userRoles.FirstOrDefault() == EnumRole.CSO.ToString())
            {
                _mapper.Map(userView?.Human?.CSO, userModel);
            }

            userModel.Roles = userRoles;
            methodResult.Result = userModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
