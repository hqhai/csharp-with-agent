// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.HomeWorkQuery
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.Curriculums;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchHomeWorkForCurriculumQuery : SearchHomeWorkForCurriculumQueryModel, IRequest<MethodResult<PagingItemsModel<HomeWorkSearchModel>>>
    {
    }

    public class SearchHomeWorkForCurriculumQueryHandler : IRequestHandler<SearchHomeWorkForCurriculumQuery, MethodResult<PagingItemsModel<HomeWorkSearchModel>>>
    {
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly IHomeWorkConfigRepository _homeWorkConfigRepository;
        private readonly ISkillRepository _skillRepository;
        private readonly ILevelRepository _levelRepository;
        private readonly ICategoryRepository _categoryRepository;

        public SearchHomeWorkForCurriculumQueryHandler(IHomeWorkRepository homeWorkRepository, IHomeWorkConfigRepository homeWorkConfigRepository, ISkillRepository skillRepository, ILevelRepository levelRepository, ICategoryRepository categoryRepository)
        {
            _homeWorkRepository = homeWorkRepository;
            _homeWorkConfigRepository = homeWorkConfigRepository;
            _skillRepository = skillRepository;
            _levelRepository = levelRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<MethodResult<PagingItemsModel<HomeWorkSearchModel>>> Handle(SearchHomeWorkForCurriculumQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PagingItemsModel<HomeWorkSearchModel>> methodResult = new MethodResult<PagingItemsModel<HomeWorkSearchModel>>();

            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var query = from h in _homeWorkRepository.Queryable.Where(p => !p.IsArchive && p.Type == EnumHomeWorkType.HomeworkExtra)
                        join p in _categoryRepository.Queryable.Include(x => x.CategoryParent) on h.ProgramId equals p.Id
                        join s in _skillRepository.Queryable on h.SkillId equals s.Id
                        join l in _levelRepository.Queryable on h.LevelId equals l.Id
                        select new HomeWorkSearchModel
                        {
                            Id = h.Id,
                            Code = h.Code,
                            Name = h.Name,
                            CreatedFullName = h.CreatedFullName,
                            CreatedDate = h.CreatedDate,
                            IsActive = h.LessonHomeWorks.Any(),
                            LevelId = l.Id,
                            Level = l.Name,
                            SkillId = s.Id,
                            SkillName = s.Name,
                            CreatedUserId = h.CreatedUserId,
                            ProgramId = h.ProgramId,
                            SubjectId = p.ParentId,
                            Subject = p.CategoryParent != null ? p.CategoryParent.Name : null,
                        };

            var homeworkIds = await _homeWorkConfigRepository.Queryable.Where(p => p.CurriculumId == request.CurriculumId).Select(x => x.HomeWorkId).ToListAsync(cancellationToken);

            query = query.Where(x => !homeworkIds.Contains(x.Id));

            request.Keyword = request.Keyword?.Trim().ToLower(CultureInfo.CurrentCulture);

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                if (Guid.TryParse(request.Keyword, out var guid))
                {
                    query = query.Where(m => m.Id == guid);
                }
                else
                {
                    query = query.Where(m => (m.Code != null && m.Code.Contains(request.Keyword)) || (m.Name != null && m.Name.Contains(request.Keyword)));
                }
            }

            var programIds = query.Where(p => p.ProgramId.HasValue).Select(p => p.ProgramId ?? default).ToList();

            var programs = await _categoryRepository.Queryable.Include(p => p.CategoryParent).WhereBulkContains(programIds, p => p.Id).ToListAsync(cancellationToken);

            if (request.SubjectIds != null && request.SubjectIds.Any())
            {
                query = query.Where(p => p.SubjectId.HasValue && request.SubjectIds.Contains(p.SubjectId.Value));
            }

            if (request.ProgramIds != null && request.ProgramIds.Any())
            {
                query = query.Where(p => p.ProgramId.HasValue && request.ProgramIds.Contains(p.ProgramId.Value));
            }

            if (request.LevelIds != null && request.LevelIds.Any())
            {
                query = query.Where(p => p.LevelId.HasValue && request.LevelIds.Contains(p.LevelId.Value));
            }

            if (request.CreatedUserIds != null && request.CreatedUserIds.Any())
            {
                query = query.Where(m => request.CreatedUserIds.Contains(m.CreatedUserId));
            }

            int totalItem = await query.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await query
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            methodResult.Result = new PagingItemsModel<HomeWorkSearchModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
