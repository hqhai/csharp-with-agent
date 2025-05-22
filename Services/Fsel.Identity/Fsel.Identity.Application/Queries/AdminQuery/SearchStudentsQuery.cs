// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.AdminQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Base.Managers;
    using Fsel.Core.Extensions;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Identity.Domain.Models.QueryModels.Students;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchStudentsQuery : SearchStudentsQueryModel, IRequest<MethodResult<PagingItemsModel<StudentSearchAdminModel>>>
    {
    }

    public class SearchStudentsQueryHandler : IRequestHandler<SearchStudentsQuery, MethodResult<PagingItemsModel<StudentSearchAdminModel>>>
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IUserSchoolRepository _userSchoolRepository;
        private readonly AuthContext _authContext;
        private readonly UserManager<User> _userManager;

        public SearchStudentsQueryHandler(IStudentRepository studentRepository,
            IUserSchoolRepository userSchoolRepository,
            AuthContext authContext,
            UserManager<User> userManager)
        {
            _studentRepository = studentRepository;
            _userSchoolRepository = userSchoolRepository;
            _authContext = authContext;
            _userManager = userManager;
        }

        public async Task<MethodResult<PagingItemsModel<StudentSearchAdminModel>>> Handle(SearchStudentsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<StudentSearchAdminModel>>();
            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var query = from u in _userManager.Users
                        join s in _studentRepository.Queryable on u.Id equals s.UserId
                        select new { User = u, Student = s };

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                request.Keyword = request.Keyword.Trim().ToLower(System.Globalization.CultureInfo.CurrentCulture);
                if (request.Keyword.IsValidEmail())
                {
                    query = query.Where(m => m.User.Email == request.Keyword);
                }
                else if (request.Keyword.IsValidPhoneNumber())
                {
                    query = query.Where(m => m.User.PhoneNumber == request.Keyword);
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
            if (!string.IsNullOrEmpty(request.SchoolName))
            {
                request.SchoolName = request.SchoolName.Trim().ToLower(System.Globalization.CultureInfo.CurrentCulture);
                query = query.Where(m => m.Student.School != null && m.Student.School == request.SchoolName);
            }
            if (request.Grades != null && request.Grades.Count > 0)
            {
                query = query.WhereBulkContains(request.Grades, x => x.Student.SchoolGrade);
            }
            if (request.Classes != null && request.Classes.Count > 0)
            {
                query = query.WhereBulkContains(request.Classes, x => x.Student.SchoolClass);
            }
            if (request.IsCourseProcess)
            {
                query = query.Where(x => x.Student.CourseId.HasValue);
            }
            if (request.CourseType.HasValue)
            {
                var courseLevels = request.CourseType.GetEnumCourseLevels();
                query = query.Where(x => x.Student.CourseLevel.HasValue && courseLevels.Contains(x.Student.CourseLevel.Value));
            }
            if (request.CourseLevel.HasValue)
            {
                query = query.Where(m => m.Student.CourseLevel == request.CourseLevel);
            }
            if (_authContext.Roles != null && _authContext.Roles.Contains(EnumRole.AdminSchool.ToString()))
            {
                var schoolId = await _userSchoolRepository.GetSchoolIdAsync();
                query = query.Where(x => x.Student.SchoolId.HasValue && x.Student.SchoolId == schoolId);
            }

            var dataQuery = query.Select(x => new StudentSearchAdminModel
            {
                Id = x.Student.Id,
                CreatedDate = x.Student.CreatedDate,
                Birthday = x.User!.Birthday,
                CourseLevel = x.Student.CourseLevel,
                PhoneNumber = x.User.PhoneNumber,
                Email = x.User.Email,
                FullName = x.User.FullName,
                Type = x.Student.CourseLevel.GetEnumCourseType(),
                CourseId = x.Student.CourseId,
                SchoolId = x.Student.SchoolId,
                SchoolName = x.Student.School,
                Class = x.Student.SchoolClass,
                Grade = x.Student.SchoolGrade,
                UserName = x.User.UserName,
                PasswordDefault = x.User.DefaultPassword,
                ExpiredDate = x.Student.ExpiredDate
            });
            int totalItem = await dataQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await dataQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            methodResult.Result = new PagingItemsModel<StudentSearchAdminModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
