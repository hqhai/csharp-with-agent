// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.CurriculumQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchCurriculumQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<CurriculumModel>>>
    {
        public IList<Guid>? CourseIds { get; set; }
        public EnumCurriculumStatus? Status { get; set; }
    }

    public class SearchCurriculumQueryHandler : IRequestHandler<SearchCurriculumQuery, MethodResult<PagingItemsModel<CurriculumModel>>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ICurriculumRepository _curriculumRepository;
        private readonly ICurriculumStudentRepository _curriculumStudentRepository;
        private readonly AuthContext _authContext;

        public SearchCurriculumQueryHandler(ICourseRepository courseRepository, ICurriculumRepository curriculumRepository, ICurriculumStudentRepository curriculumStudentRepository, AuthContext authContext)
        {
            _courseRepository = courseRepository;
            _curriculumRepository = curriculumRepository;
            _curriculumStudentRepository = curriculumStudentRepository;
            _authContext = authContext;
        }

        public async Task<MethodResult<PagingItemsModel<CurriculumModel>>> Handle(SearchCurriculumQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<CurriculumModel>>();

            var schoolIdStr = _authContext.ClaimsPrincipal?.FindFirstValue("SchoolId");

            if (!string.IsNullOrEmpty(schoolIdStr) || !Guid.TryParse(schoolIdStr, out Guid schoolId))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(schoolId), _authContext.CurrentUserId);
                return methodResult;
            }

            var query = await (from baseQuery in _curriculumRepository.Queryable
                               join c in _courseRepository.Queryable on baseQuery.CourseId equals c.Id
                               join cc in _courseRepository.Queryable on baseQuery.CourseCloneId equals cc.Id
                               where baseQuery.SchoolId == schoolId
                               select new
                               {
                                   Curriculum = baseQuery,
                                   Course = c,
                                   CourseClone = cc,
                               }).ToListAsync(cancellationToken);

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                query = query.Where(p => !string.IsNullOrEmpty(p.Curriculum.CurriculumName) && p.Curriculum.CurriculumName.Contains(request.Keyword, StringComparison.CurrentCultureIgnoreCase) || !string.IsNullOrEmpty(p.Course.Name) && p.Course.Name.Contains(request.Keyword, StringComparison.CurrentCultureIgnoreCase)).ToList();
            }

            if (request.CourseIds != null && request.CourseIds.Any())
            {
                query = query.Where(p => request.CourseIds.Contains(p.Course.Id)).ToList();
            }

            if (request.Status.HasValue)
            {
                query = query.Where(p => p.Curriculum.CurriculumStatus == request.Status).ToList();
            }

            var curriculums = query.Select(p => new CurriculumModel()
            {
                Id = p.Curriculum.Id,
                CurriculumName = p.Curriculum.CurriculumName,
                CourseId = p.Curriculum.CourseId,
                CourseCloneId = p.Curriculum.CourseCloneId,
                CreatedDate = p.Curriculum.CreatedDate,
                CourseName = p.Course.Name,
                StartDate = p.Curriculum.StartDate,
                EndDate = p.Curriculum.EndDate,
                CourseLevel = p.Course.CourseLevel,
                CourseType = p.Course.CourseType,
                Subject = "Tiếng Anh"
            }).ToList();

            int totalItem = curriculums.Count;
            var lists = curriculums
                    .ApplySortAndPaging(request)
                    .ToList();

            var curriculumIds = lists.Select(p => p.Id).ToList();

            var curriculumStudents = await _curriculumStudentRepository.Queryable.WhereBulkContains(curriculumIds, p => p.Id).ToListAsync(cancellationToken);

            lists.ForEach(p =>
            {
                p.NumberOfStudent = curriculumStudents.Where(x => x.CurriculumId == p.Id).Count();
            });

            methodResult.Result = new PagingItemsModel<CurriculumModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
