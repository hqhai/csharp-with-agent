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
        public IList<Guid>? SubjectIds { get; set; }
        public IList<Guid>? ProgramIds { get; set; }
        public IList<Guid>? LevelIds { get; set; }
        public IList<Guid>? CourseIds { get; set; }
        public IList<EnumCurriculumStatus>? Status { get; set; }
    }

    public class SearchCurriculumQueryHandler : IRequestHandler<SearchCurriculumQuery, MethodResult<PagingItemsModel<CurriculumModel>>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ICurriculumRepository _curriculumRepository;
        private readonly ICurriculumStudentRepository _curriculumStudentRepository;
        private readonly AuthContext _authContext;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILevelRepository _levelRepository;

        public SearchCurriculumQueryHandler(ICourseRepository courseRepository, ICurriculumRepository curriculumRepository, ICurriculumStudentRepository curriculumStudentRepository, AuthContext authContext, ICategoryRepository categoryRepository, ILevelRepository levelRepository)
        {
            _courseRepository = courseRepository;
            _curriculumRepository = curriculumRepository;
            _curriculumStudentRepository = curriculumStudentRepository;
            _authContext = authContext;
            _categoryRepository = categoryRepository;
            _levelRepository = levelRepository;
        }

        public async Task<MethodResult<PagingItemsModel<CurriculumModel>>> Handle(SearchCurriculumQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<CurriculumModel>>();

            var schoolIdStr = _authContext.ClaimsPrincipal?.FindFirstValue("SchoolId");

            if (string.IsNullOrEmpty(schoolIdStr) || !Guid.TryParse(schoolIdStr, out Guid schoolId))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(schoolId), _authContext.CurrentUserId);
                return methodResult;
            }

            var query = await (from baseQuery in _curriculumRepository.Queryable
                               join c in _courseRepository.Queryable on baseQuery.CourseId equals c.Id
                               join cc in _courseRepository.Queryable on baseQuery.CourseCloneId equals cc.Id
                               join l in _levelRepository.Queryable on c.LevelId equals l.Id
                               where baseQuery.SchoolId == schoolId
                               select new
                               {
                                   Curriculum = baseQuery,
                                   Course = c,
                                   CourseClone = cc,
                                   Level = l
                               }).ToListAsync(cancellationToken);

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                query = query.Where(p => !string.IsNullOrEmpty(p.Curriculum.CurriculumName) && p.Curriculum.CurriculumName.Contains(request.Keyword, StringComparison.CurrentCultureIgnoreCase) || !string.IsNullOrEmpty(p.Course.Name) && p.Course.Name.Contains(request.Keyword, StringComparison.CurrentCultureIgnoreCase)).ToList();
            }

            if (request.Status != null && request.Status.Any())
            {
                query = query.Where(p => request.Status.Contains(p.Curriculum.CurriculumStatus)).ToList();
            }

            var programIds = query.Where(p => p.CourseClone.ProgramId.HasValue).Select(p => p.CourseClone.ProgramId ?? default).ToList();

            var programs = await _categoryRepository.Queryable.Include(p => p.CategoryParent).WhereBulkContains(programIds, p => p.Id).ToListAsync(cancellationToken);

            var curriculums = query.Select(p =>
            {
                var program = programs.FirstOrDefault(x => x.Id == p.CourseClone.ProgramId);

                return new CurriculumModel()
                {
                    Id = p.Curriculum.Id,
                    CurriculumName = p.Curriculum.CurriculumName,
                    CourseId = p.Curriculum.CourseId,
                    CourseCloneId = p.Curriculum.CourseCloneId,
                    CreatedDate = p.Curriculum.CreatedDate,
                    CourseName = p.Course.Name,
                    StartDate = p.Curriculum.StartDate,
                    EndDate = p.Curriculum.EndDate,
                    Level = p.Level.Name,
                    Program = program?.Name,
                    Subject = program?.CategoryParent?.Name,
                    ProgramId = program?.Id,
                    SubjectId = program?.CategoryParent?.Id,
                    LevelId = p.Level.Id,
                };
            }).ToList();

            if (request.SubjectIds != null && request.SubjectIds.Any())
            {
                curriculums = curriculums.Where(p => p.SubjectId.HasValue && request.SubjectIds.Contains(p.SubjectId.Value)).ToList();
            }

            if (request.ProgramIds != null && request.ProgramIds.Any())
            {
                curriculums = curriculums.Where(p => p.ProgramId.HasValue && request.ProgramIds.Contains(p.ProgramId.Value)).ToList();
            }

            if (request.LevelIds != null && request.LevelIds.Any())
            {
                curriculums = curriculums.Where(p => request.LevelIds.Contains(p.LevelId)).ToList();
            }

            if (request.CourseIds != null && request.CourseIds.Any())
            {
                curriculums = curriculums.Where(p => request.CourseIds.Contains(p.CourseCloneId)).ToList();
            }

            int totalItem = curriculums.Count;
            var lists = curriculums
                    .ApplySortAndPaging(request)
                    .ToList();

            var curriculumIds = lists.Select(p => p.Id).ToList();

            var curriculumStudents = await _curriculumStudentRepository.Queryable.WhereBulkContains(curriculumIds, p => p.CurriculumId).ToListAsync(cancellationToken);

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
