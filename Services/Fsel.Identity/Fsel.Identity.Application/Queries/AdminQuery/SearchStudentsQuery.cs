// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.AdminQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
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

        public SearchStudentsQueryHandler(IStudentRepository studentRepository,
            IUserSchoolRepository userSchoolRepository,
            AuthContext authContext)
        {
            _studentRepository = studentRepository;
            _userSchoolRepository = userSchoolRepository;
            _authContext = authContext;
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
            var query = _studentRepository.Queryable;

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                request.Keyword = request.Keyword.Trim().ToLower(System.Globalization.CultureInfo.CurrentCulture);
                if (request.Keyword.IsValidEmail())
                {
                    query = query.Where(m => m.Human != null && m.Human.Email!.Contains(request.Keyword));
                }
                else if (request.Keyword.IsValidPhoneNumber())
                {
                    query = query.Where(m => m.Human != null && m.Human.PhoneNumber == request.Keyword);
                }
                else if (Guid.TryParse(request.Keyword, out var guid))
                {
                    query = query.Where(m => m.Id == guid);
                }
                else
                {
                    query = query.Where(m => m.Human != null && m.Human.FullName!.Contains(request.Keyword));
                }
            }
            if (!string.IsNullOrEmpty(request.SchoolName))
            {
                request.SchoolName = request.SchoolName.Trim().ToLower(System.Globalization.CultureInfo.CurrentCulture);
                query = query.Where(m => m.School != null && m.School.Contains(request.SchoolName));
            }
            if (request.Grades != null && request.Grades.Count > 0)
            {
                query = query.WhereBulkContains(request.Grades, x => x.SchoolGrade);
            }
            if (request.Classes != null && request.Classes.Count > 0)
            {
                query = query.WhereBulkContains(request.Classes, x => x.SchoolClass);
            }
            if (_authContext.Roles != null && _authContext.Roles.Contains(EnumRole.AdminSchool.ToString()))
            {
                var schoolId = await _userSchoolRepository.GetSchoolIdAsync();
                query = query.Where(x => x.SchoolId.HasValue && x.SchoolId == schoolId);
            }

            var dataQuery = query.Select(x => new StudentSearchAdminModel
            {
                Id = x.Id,
                CreatedDate = x.CreatedDate,
                Birthday = x.Human!.Birthday,
                CourseLevel = x.CourseLevel,
                PhoneNumber = x.Human.PhoneNumber,
                Email = x.Human.Email,
                FullName = x.Human.FullName,
                Type = x.CourseLevel.GetEnumCourseType(),
                SchoolId = x.SchoolId,
                SchoolName = x.School,
                Class = x.SchoolClass,
                Grade = x.SchoolGrade,
                UserName = x.Human.User!.UserName
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
