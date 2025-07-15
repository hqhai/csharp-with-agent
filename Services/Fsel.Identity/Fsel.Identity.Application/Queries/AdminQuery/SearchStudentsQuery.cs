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
        private readonly IHumanRepository _humanRepository;

        public SearchStudentsQueryHandler(IStudentRepository studentRepository,
            IUserSchoolRepository userSchoolRepository,
            AuthContext authContext,
            UserManager<User> userManager,
            IHumanRepository humanRepository)
        {
            _studentRepository = studentRepository;
            _userSchoolRepository = userSchoolRepository;
            _authContext = authContext;
            _userManager = userManager;
            _humanRepository = humanRepository;
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

            var queryStudent = _studentRepository.Queryable;
            if (request.Grades != null && request.Grades.Count > 0)
            {
                queryStudent = queryStudent.WhereBulkContains(request.Grades, x => x.SchoolGrade);
            }
            if (request.Classes != null && request.Classes.Count > 0)
            {
                queryStudent = queryStudent.WhereBulkContains(request.Classes, x => x.SchoolClass);
            }
            if (!string.IsNullOrEmpty(request.SchoolName))
            {
                request.SchoolName = request.SchoolName.Trim().ToLower(System.Globalization.CultureInfo.CurrentCulture);
                queryStudent = queryStudent.Where(m => m.School != null && m.School == request.SchoolName);
            }
            if (request.IsCourseProcess)
            {
                queryStudent = queryStudent.Where(x => x.CourseId.HasValue);
            }
            if (request.CourseType.HasValue)
            {
                var courseLevels = request.CourseType.GetEnumCourseLevels();
                queryStudent = queryStudent.Where(x => x.CourseLevel.HasValue && courseLevels.Contains(x.CourseLevel.Value));
            }
            if (request.CourseLevel.HasValue)
            {
                queryStudent = queryStudent.Where(m => m.CourseLevel == request.CourseLevel);
            }
            if (_authContext.Roles != null && _authContext.Roles.Contains(EnumRole.AdminSchool.ToString()))
            {
                var schoolId = await _userSchoolRepository.GetSchoolIdAsync();
                queryStudent = queryStudent.Where(x => x.SchoolId.HasValue && x.SchoolId == schoolId);
            }

            var query = from u in _userManager.Users
                        join h in _humanRepository.Queryable on u.Id equals h.UserId
                        join s in queryStudent on h.Id equals s.HumanId
                        select new { User = u, Human = h, Student = s };

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                request.Keyword = request.Keyword.Trim().ToLower(System.Globalization.CultureInfo.CurrentCulture);
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

            var dataQuery = query.Select(x => new StudentSearchAdminModel
            {
                Id = x.Student.Id,
                CreatedDate = x.Student.CreatedDate,
                Birthday = x.Human!.Birthday,
                CourseLevel = x.Student.CourseLevel,
                PhoneNumber = x.Human.PhoneNumber,
                Email = x.Human.Email,
                FullName = x.Human.FullName,
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
