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
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Identity.Infrastructure.Repositories;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchStudentsQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<StudentSearchAdminModel>>>
    {
    }

    public class SearchStudentsQueryHandler : IRequestHandler<SearchStudentsQuery, MethodResult<PagingItemsModel<StudentSearchAdminModel>>>
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IUserSchoolRepository _userSchoolRepository;

        public SearchStudentsQueryHandler(IStudentRepository studentRepository, IUserSchoolRepository userSchoolRepository)
        {
            _studentRepository = studentRepository;
            _userSchoolRepository = userSchoolRepository;
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
            var schoolId = await _userSchoolRepository.GetSchoolIdAsync();
            var query = _studentRepository.Queryable.Include(x => x.Human).Select(x => new StudentSearchAdminModel
            {
                Id = x.Id,
                CreatedDate = x.CreatedDate,
                Birthday = x.Human!.Birthday,
                CourseLevel = x.CourseLevel,
                Email = x.Human.Email,
                FullName = x.Human.FullName,
                Type = x.CourseLevel.GetEnumCourseType(),
                SchoolId = x.SchoolId
            });

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                if (request.Keyword.IsValidEmail())
                {
                    query = query.Where(m => (m.Email ?? string.Empty).Trim().ToLower().Contains(request.Keyword.Trim().ToLower()));
                }
                else
                {
                    query = query.Where(m => m.Id.ToString() == request.Keyword || (m.FullName ?? string.Empty).Trim().ToLower().Contains(request.Keyword.Trim().ToLower()));
                }
            }
            if (schoolId.HasValue)
            {
                query = query.Where(x => x.SchoolId == schoolId.Value);
            }

            int totalItem = await query.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await query
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
