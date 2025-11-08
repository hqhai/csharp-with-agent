// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.UserQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Application.Services.LmsCourseService;
    using Fsel.Identity.Application.Services.OrderService;
    using Fsel.Identity.Application.Services.SystemService;
    using Fsel.Identity.Application.Services.TrainingService;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
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
        private readonly ISystemService _systemService;
        private readonly ISystemConfigRepository _systemConfigRepository;

        public GetUserProfileQueryHandler(IMapper mapper,
            AuthContext authContext,
            UserManager<User> userManager,
            ITrainingService trainingService,
            IOrderService orderService,
            ILmsCourseService lmsCourseService,
            ISystemService systemService,
            ISystemConfigRepository systemConfigRepository)
        {
            _mapper = mapper;
            _authContext = authContext;
            _userManager = userManager;
            _trainingService = trainingService;
            _orderService = orderService;
            _lmsCourseService = lmsCourseService;
            _systemService = systemService;
            _systemConfigRepository = systemConfigRepository;
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
            var targetRoles = new List<string> { EnumRole.AdminSchool.ToString(), EnumRole.TeacherCampus.ToString(), EnumRole.AdminCampus.ToString() };
            var hasMatchedRole = targetRoles.Any(r => r == userRoles.FirstOrDefault());

            User? userView = null;
            if (userRoles.FirstOrDefault() == EnumRole.CSO.ToString())
            {
                userView = await _userManager.Users.Include(x => x!.CSO)
                                                   .FirstOrDefaultAsync(x => x.Id == _authContext.CurrentUserId, cancellationToken);
            }
            else if (userRoles.FirstOrDefault() == EnumRole.Teacher.ToString())
            {
                userView = await _userManager.Users.Include(x => x!.Teacher)
                                                   .ThenInclude(x => x!.TeacherBankAccounts)
                                                   .FirstOrDefaultAsync(x => x.Id == _authContext.CurrentUserId, cancellationToken);
            }
            else if (userRoles.FirstOrDefault() == EnumRole.Student.ToString() || userRoles.FirstOrDefault() == EnumRole.StudentCampus.ToString())
            {
                userView = await _userManager.Users.Include(x => x!.Student)
                                                   .ThenInclude(x => x!.ParentStudents)
                                                   .Include(p => p.Receiver).ThenInclude(p => p.Sender)
                                                   .FirstOrDefaultAsync(x => x.Id == _authContext.CurrentUserId, cancellationToken);
                var student = userView?.Student;
                if (student != null && student.ParentStudents != null && student.ParentStudents.Count > 0)
                {
                    userView = await _userManager.Users.Include(x => x!.Student)
                                                  .ThenInclude(x => x!.ParentStudents)
                                                  .ThenInclude(x => x!.Parent)
                                                  .ThenInclude(x => x!.User)
                                                  .Include(p => p.Receiver).ThenInclude(p => p.Sender)
                                                  .FirstOrDefaultAsync(x => x.Id == _authContext.CurrentUserId, cancellationToken);
                }
            }
            else if (userRoles.FirstOrDefault() == EnumRole.Parent.ToString())
            {
                userView = await _userManager.Users.Include(x => x!.Parent)
                                                   .ThenInclude(x => x!.ParentStudents)
                                                   .FirstOrDefaultAsync(x => x.Id == _authContext.CurrentUserId, cancellationToken);
                var parentStudents = userView?.Parent?.ParentStudents;
                if (parentStudents != null && parentStudents.Count > 0)
                {
                    userView = await _userManager.Users.Include(x => x!.Parent)
                                                                      .ThenInclude(x => x!.ParentStudents)
                                                                      .ThenInclude(x => x.Student)
                                                                      .ThenInclude(x => x!.User)
                                                                      .FirstOrDefaultAsync(x => x.Id == _authContext.CurrentUserId, cancellationToken);
                }
            }
            else if (hasMatchedRole)
            {
                userView = await _userManager.Users.Include(x => x.UserSchools)
                                                   .FirstOrDefaultAsync(x => x.Id == _authContext.CurrentUserId && x.EmailConfirmed, cancellationToken);
            }
            else if (userRoles.FirstOrDefault() == EnumRole.Moderator.ToString() || userRoles.FirstOrDefault() == EnumRole.MasterAdmin.ToString() || userRoles.FirstOrDefault() == EnumRole.Admin.ToString())
            {
                userView = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == _authContext.CurrentUserId, cancellationToken);
            }
            var isEnabledExtra = await _systemConfigRepository.Queryable.Select(x => x.IsEnabled).FirstOrDefaultAsync(cancellationToken);

            var userModel = _mapper.Map<UserProfileModel>(userView ?? user);
            userModel.IsEnabledExtra = isEnabledExtra;
            _mapper.Map(userView, userModel);

            if (userRoles.FirstOrDefault() == EnumRole.Student.ToString() || userRoles.FirstOrDefault() == EnumRole.StudentCampus.ToString())
            {
                var student = userView?.Student;
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
                        var classStudent = await _trainingService.GetClassToStudentId(student.Id);
                        if (!classStudent.IsSuccessStatusCode)
                        {
                            methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallTrainingServiceError), nameof(classStudent));
                            return methodResult;
                        }

                        //if (student.SchoolId != null)
                        //{
                        //    var schoolResult = await _systemService.ExecuteListSchoolQueryAsync(new BaseQueryModel
                        //    {
                        //        Filters = new List<GenericFilterModel>() { new GenericFilterModel { Property = "Id", Operator = Common.Enums.EnumFilterOperator.Equal, Value = student.SchoolId } },
                        //        IncludePaths = new List<string>() { "School" }
                        //    });
                        //    if (!schoolResult.IsSuccessStatusCode || schoolResult.Content?.Result == null)
                        //    {
                        //        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                        //        return methodResult;
                        //    }
                        //    student.School = schoolResult.Content?.Result?.FirstOrDefault(x => x.Id == student.SchoolId)?.Name;
                        //}

                        _mapper.Map(student, userModel);
                        userModel.CodeClass = classStudent.Content?.Result?.Code;
                        userModel.School = student.School?.ToString();
                        userModel.SchoolName = student.School?.ToString();

                        if (student.ParentStudents.Count > 0)
                        {
                            var parent = student.ParentStudents.FirstOrDefault()?.Parent;
                            if (parent != null)
                            {
                                userModel.Parent = _mapper.Map<ParentProfileModel>(parent.User);
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

                        /*var schoolResult = await _systemService.ExecuteListSchoolQueryAsync(new BaseQueryModel
                        {
                            Filters = new List<GenericFilterModel>
                            {
                                new GenericFilterModel
                                {
                                    Property = nameof(student.SchoolId),
                                    Value = student.SchoolId,
                                    Operator = Common.Enums.EnumFilterOperator.Equal
                                }
                            }
                        });
                        var school = schoolResult.Content?.Result;
                        userModel.SchoolName = school?.FirstOrDefault(x => x.Id == student.SchoolId)?.Name;*/
                    }
                    if (userView?.Receiver?.Sender != null)
                    {
                        userModel.Sender = new SenderModel()
                        {
                            SenderId = userView.Receiver.Sender.Id,
                            FullName = userView.Receiver.Sender.FullName,
                            Code = userView.Receiver.Sender.Code
                        };
                    }
                }
            }
            if (userRoles.FirstOrDefault() == EnumRole.Parent.ToString())
            {
                var parent = userView?.Parent;
                if (parent != null && parent.ParentStudents != null && parent.ParentStudents.Count > 0)
                {
                    userModel.Students = _mapper.Map<List<StudentProfileModel>>(parent.ParentStudents.Select(x => x.Student).ToList());
                    _mapper.Map(parent, userModel);

                    foreach (var student in userModel.Students)
                    {
                        var classStudent = await _trainingService.GetClassToStudentId(student.Id);
                        if (!classStudent.IsSuccessStatusCode)
                        {
                            methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallTrainingServiceError), nameof(classStudent));
                            return methodResult;
                        }
                        if (classStudent.Content?.Result != null)
                        {
                            student.CodeClass = classStudent!.Content!.Result!.Code;
                        }
                        var userName = parent.ParentStudents.Select(x => x.Student).FirstOrDefault(x => x!.Id == student.Id)?.User?.UserName;
                        student.UserName = userName;
                    }
                }
            }
            if (userRoles.FirstOrDefault() == EnumRole.Teacher.ToString())
            {
                var teacher = userView?.Teacher;
                if (teacher != null)
                {
                    userModel.TeacherBankAccounts = _mapper.Map<IList<TeacherBankAccountModel>>(teacher.TeacherBankAccounts);
                    _mapper.Map(teacher, userModel);
                }
            }
            if (userRoles.FirstOrDefault() == EnumRole.CSO.ToString())
            {
                _mapper.Map(userView?.CSO, userModel);
            }

            if (hasMatchedRole)
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
            methodResult.Result = userModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
