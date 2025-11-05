// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.ManagerReportQuery
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Application.Services.SystemService;
    using Fsel.Identity.Application.Services.SystemService.QueryModels;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.QueryModels.ManagerReports;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentsQuery : SearchStudentQueryModel, IRequest<MethodResult<IList<StudentDtoModel>>>
    {
    }

    public class GetStudentsQueryHandler : IRequestHandler<GetStudentsQuery, MethodResult<IList<StudentDtoModel>>>
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IUserSchoolRepository _userSchoolRepository;
        private readonly AuthContext _authContext;
        private readonly ISystemService _systemService;
        private readonly UserManager<User> _userManager;
        private readonly IHumanRepository _humanRepository;
        private readonly ISchoolClassRepository _schoolClassRepository;

        public GetStudentsQueryHandler(IStudentRepository studentRepository,
            IUserSchoolRepository userSchoolRepository,
            AuthContext authContext,
            ISystemService systemService,
            UserManager<User> userManager,
            IHumanRepository humanRepository,
            ISchoolClassRepository schoolClassRepository)
        {
            _studentRepository = studentRepository;
            _userSchoolRepository = userSchoolRepository;
            _authContext = authContext;
            _systemService = systemService;
            _userManager = userManager;
            _humanRepository = humanRepository;
            _schoolClassRepository = schoolClassRepository;
        }

        public async Task<MethodResult<IList<StudentDtoModel>>> Handle(GetStudentsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<StudentDtoModel>> methodResult = new MethodResult<IList<StudentDtoModel>>();

            var queryStudent = _studentRepository.Queryable;
            if (request.SchoolGrades != null && request.SchoolGrades.Count > 0)
            {
                queryStudent = queryStudent.WhereBulkContains(request.SchoolGrades, x => x.SchoolGrade);
            }
            if (request.SchoolClasses != null && request.SchoolClasses.Count > 0)
            {
                queryStudent = queryStudent.WhereBulkContains(request.SchoolClasses, x => x.SchoolClass);
            }

            var targetRoles = new List<string> { EnumRole.AdminSchool.ToString(), EnumRole.TeacherCampus.ToString(), EnumRole.AdminCampus.ToString() };
            var hasMatchedRole = _authContext.Roles != null && _authContext.Roles.Any(r => targetRoles.Contains(r));
            if (hasMatchedRole)
            {
                var schoolId = await _userSchoolRepository.GetSchoolIdAsync();
                queryStudent = queryStudent.Where(x => x.SchoolId.HasValue && x.SchoolId == schoolId);
            }

            if (_authContext.Roles != null && (_authContext.Roles.Contains(EnumRole.TeacherCampus.ToString())))
            {
                var schoolClassIds = await _schoolClassRepository.Queryable.Where(p => p.TeacherId == _authContext.CurrentUserId).Select(p => p.Id).ToListAsync(cancellationToken);
                queryStudent = queryStudent.Where(x => x.SchoolClassId.HasValue && schoolClassIds.Contains(x.SchoolClassId.Value));
            }

            if (request.LearningStatuses?.Any() == true)
            {
                var hasInProgress = request.LearningStatuses.Contains(EnumLearningStatus.InProgress);
                var hasExpired = request.LearningStatuses.Contains(EnumLearningStatus.Expired);

                if (hasInProgress && !hasExpired)
                {
                    queryStudent = queryStudent.Where(x => x.ExpiredDate.HasValue && x.ExpiredDate.Value > DateTime.UtcNow);
                }
                else if (!hasInProgress && hasExpired)
                {
                    queryStudent = queryStudent.Where(x => x.ExpiredDate.HasValue && x.ExpiredDate.Value <= DateTime.UtcNow);
                }
                else if (hasInProgress && hasExpired)
                {
                    queryStudent = queryStudent.Where(x => x.ExpiredDate.HasValue);
                }
            }
            if (request.IsLearning.HasValue)
            {
                queryStudent = queryStudent.Where(x => request.IsLearning.Value ? x.CourseId.HasValue : !x.CourseId.HasValue);
            }
            if (request.CourseType.HasValue)
            {
                var courseLevels = EnumCourseLevelHelper.GetEnumCourseLevels(request.CourseType.Value);
                queryStudent = queryStudent.Where(x => x.CourseLevel.HasValue && courseLevels.Contains(x.CourseLevel.Value));
            }
            if (request.CourseLevels != null && request.CourseLevels.Any())
            {
                queryStudent = queryStudent.Where(x => x.CourseLevel.HasValue && request.CourseLevels.Contains(x.CourseLevel.Value));
            }

            if (request.CourseLevel.HasValue)
            {
                queryStudent = queryStudent.Where(x => x.CourseLevel.HasValue && x.CourseLevel == request.CourseLevel.Value);
            }
            if (request.CourseTypes != null && request.CourseTypes.Any())
            {
                var courseLevels = EnumCourseLevelHelper.GetCourseLevels(request.CourseTypes);
                queryStudent = queryStudent.Where(x => x.CourseLevel.HasValue && courseLevels.Contains(x.CourseLevel.Value));
            }

            var query = from u in _userManager.Users
                        join h in _humanRepository.Queryable on u.Id equals h.UserId
                        join s in queryStudent on h.Id equals s.HumanId
                        select new { User = u, Human = h, Student = s };

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                request.Keyword = request.Keyword.Trim().ToLower(CultureInfo.CurrentCulture);
                if (request.Keyword.IsValidEmail())
                {
                    query = query.Where(m => m.Human != null && m.Human.Email == request.Keyword);
                }
                else if (request.Keyword.IsValidPhoneNumber())
                {
                    query = query.Where(m => m.Human != null && m.Human.PhoneNumber == request.Keyword);
                }
                else if (Guid.TryParse(request.Keyword, out var guid))
                {
                    query = query.Where(m => m.Student.Id == guid);
                }
                else
                {
                    var queryUserName = query.Where(m => (m.User.UserName != null && m.User.UserName == request.Keyword));
                    var queryFullName = query.Where(m => m.User.FullName != null && EF.Functions.Contains(m.User.FullName, $"\"{request.Keyword}\"") && EF.Functions.Like(m.User.FullName, $"%{request.Keyword}%"));
                    query = queryUserName.Union(queryFullName);
                }
            }

            if (_authContext.Roles != null && _authContext.Roles.Contains(EnumRole.Admin.ToString()))
            {
                var schoolIdResults = await _systemService.GetSchoolIdsAsync(new GetSchoolsQueryModel
                {
                    DistrictIds = request.DistrictIds,
                    ProvinceIds = request.ProvinceIds,
                    SchoolIds = request.SchoolIds,
                });
                var schoolIds = schoolIdResults.Content?.Result;
                query = query.Where(x => x.Student.SchoolId.HasValue && schoolIds != null && schoolIds.Contains(x.Student.SchoolId.Value));
            }
            var dataQuery = query.Select(i => new StudentDtoModel
            {
                Id = i.Student.Id,
                FullName = i.Human!.FullName,
                BirthDay = i.Human.Birthday,
                Email = i.Human.Email,
                PhoneNumber = i.Human.PhoneNumber,
                CourseLevel = i.Student.CourseLevel,
                ExpiredDate = i.Student.ExpiredDate,
                School = i.Student.School,
                SchoolClass = i.Student.SchoolClass,
                SchoolGrade = i.Student.SchoolGrade,
                BaseCourseLevel = i.Student.BaseCourseLevel,
                SchoolId = i.Student.SchoolId,
                CourseId = i.Student.CourseId,
                UserId = i.Human.UserId,
                CreatedDate = i.Student.CreatedDate,
                UserName = i.User.UserName
            });

            var list = await dataQuery.AsNoTracking().ToListAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            methodResult.Result = list.OrderBy(x => int.TryParse(x.SchoolGrade, out int graded) ? graded : 0).ThenBy(x => x.SchoolClass).ThenBy(x => x.FullName).ToList();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
