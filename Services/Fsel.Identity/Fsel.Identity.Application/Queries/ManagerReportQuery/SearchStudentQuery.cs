// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.ManagerReportQuery
{
    using System.Diagnostics;
    using System.Globalization;
    using System.Text.RegularExpressions;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Base.Managers;
    using Fsel.Core.Extensions;
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

    public class SearchStudentQuery : SearchStudentQueryModel, IRequest<MethodResult<PagingItemsModel<StudentDtoModel>>>
    {
    }

    public class SearchStudentQueryHandler : IRequestHandler<SearchStudentQuery, MethodResult<PagingItemsModel<StudentDtoModel>>>
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IUserSchoolRepository _userSchoolRepository;
        private readonly AuthContext _authContext;
        private readonly ISystemService _systemService;
        private readonly UserManager<User> _userManager;

        public SearchStudentQueryHandler(IStudentRepository studentRepository,
            IUserSchoolRepository userSchoolRepository,
            AuthContext authContext,
            ISystemService systemService,
            UserManager<User> userManager)
        {
            _studentRepository = studentRepository;
            _userSchoolRepository = userSchoolRepository;
            _authContext = authContext;
            _systemService = systemService;
            _userManager = userManager;
        }

        public async Task<MethodResult<PagingItemsModel<StudentDtoModel>>> Handle(SearchStudentQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PagingItemsModel<StudentDtoModel>> methodResult = new MethodResult<PagingItemsModel<StudentDtoModel>>();
            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            var queryStudent = _studentRepository.Queryable;
            if (request.SchoolGrades != null && request.SchoolGrades.Count > 0)
            {
                queryStudent = queryStudent.WhereBulkContains(request.SchoolGrades, x => x.SchoolGrade);
            }
            if (request.SchoolClasses != null && request.SchoolClasses.Count > 0)
            {
                queryStudent = queryStudent.WhereBulkContains(request.SchoolClasses, x => x.SchoolClass);
            }
            if (!string.IsNullOrEmpty(request.SchoolGrade))
            {
                request.SchoolGrade = request.SchoolGrade.Trim().ToLower(CultureInfo.CurrentCulture);
                queryStudent = queryStudent.Where(x => x.SchoolGrade == request.SchoolGrade);
            }
            if (!string.IsNullOrEmpty(request.SchoolClass))
            {
                request.SchoolClass = request.SchoolClass.Trim().ToLower(CultureInfo.CurrentCulture);
                queryStudent = queryStudent.Where(x => x.SchoolClass == request.SchoolClass);
            }
            if (_authContext.Roles != null && _authContext.Roles.Contains(EnumRole.AdminSchool.ToString()))
            {
                var schoolId = await _userSchoolRepository.GetSchoolIdAsync();
                queryStudent = queryStudent.Where(x => x.SchoolId.HasValue && x.SchoolId == schoolId);
            }
            if (request.LearningStatus.HasValue)
            {
                if (request.LearningStatus.Value == EnumLearningStatus.InProgress)
                {
                    queryStudent = queryStudent.Where(x => x.ExpiredDate.HasValue && x.ExpiredDate.Value > DateTime.UtcNow);
                }
                else
                {
                    queryStudent = queryStudent.Where(x => x.ExpiredDate.HasValue && x.ExpiredDate.Value <= DateTime.UtcNow);
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

            var query = from u in _userManager.Users
                        join s in queryStudent on u.Id equals s.UserId
                        select new { User = u, Student = s };

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                request.Keyword = request.Keyword.Trim().ToLower(CultureInfo.CurrentCulture);
                if (request.Keyword.IsValidEmail())
                {
                    query = query.Where(m => m.User != null && m.User.Email == request.Keyword);
                }
                else if (request.Keyword.IsValidPhoneNumber())
                {
                    query = query.Where(m => m.User != null && m.User.PhoneNumber == request.Keyword);
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
                FullName = i.User!.FullName,
                BirthDay = i.User.Birthday,
                Email = i.User.Email,
                PhoneNumber = i.User.PhoneNumber,
                CourseLevel = i.Student.CourseLevel,
                ExpiredDate = i.Student.ExpiredDate,
                School = i.Student.School,
                SchoolClass = i.Student.SchoolClass,
                SchoolGrade = i.Student.SchoolGrade,
                SchoolId = i.Student.SchoolId,
                CourseId = i.Student.CourseId,
                UserId = i.Student.UserId,
                CreatedDate = i.Student.CreatedDate,
                BaseCourseLevel = i.Student.BaseCourseLevel,
                UserName = i.User!.UserName
            });
            int totalItem = await dataQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = (await dataQuery.ToListAsync(cancellationToken))
                         .OrderBy(x => !string.IsNullOrEmpty(x.SchoolGrade) ? ExtractNumber(x.SchoolGrade) : 0)
                         .ThenBy(x => x.SchoolClass)
                         .ThenBy(x => x.FullName)
                         .ApplyPaging(request)
                         .ToList();

            methodResult.Result = new PagingItemsModel<StudentDtoModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private static int ExtractNumber(string input)
        {
            var match = Regex.Match(input, @"\d+");
            return match.Success ? int.Parse(match.Value, CultureInfo.CurrentCulture) : int.MaxValue;
        }
    }
}
