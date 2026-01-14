// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.HomeWorkConfigQuery
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.HomeWorkConfigs;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchHomeWorkConfigQuery : SearchHomeWorkConfigQueryModel, IRequest<MethodResult<PagingItemsModel<HomeWorkConfigModel>>>
    {
    }

    public class SearchHomeWorkConfigQueryHandler : IRequestHandler<SearchHomeWorkConfigQuery, MethodResult<PagingItemsModel<HomeWorkConfigModel>>>
    {
        private readonly IHomeWorkConfigRepository _homeWorkConfigRepository;
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILevelRepository _levelRepository;
        private readonly ISkillRepository _skillRepository;

        public SearchHomeWorkConfigQueryHandler(IHomeWorkConfigRepository homeWorkConfigRepository, IHomeWorkRepository homeWorkRepository, ICategoryRepository categoryRepository, ILevelRepository levelRepository, ISkillRepository skillRepository)
        {
            _homeWorkConfigRepository = homeWorkConfigRepository;
            _homeWorkRepository = homeWorkRepository;
            _categoryRepository = categoryRepository;
            _levelRepository = levelRepository;
            _skillRepository = skillRepository;
        }

        public async Task<MethodResult<PagingItemsModel<HomeWorkConfigModel>>> Handle(SearchHomeWorkConfigQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<HomeWorkConfigModel>>();

            var query = from hc in _homeWorkConfigRepository.Queryable
                        join h in _homeWorkRepository.Queryable on hc.HomeWorkId equals h.Id
                        join p in _categoryRepository.Queryable.Include(n => n.CategoryParent) on h.ProgramId equals p.Id
                        join s in _skillRepository.Queryable on h.SkillId equals s.Id
                        join l in _levelRepository.Queryable on h.LevelId equals l.Id
                        where hc.CurriculumId == request.CurriculumId
                        select new HomeWorkConfigModel()
                        {
                            Id = hc.Id,
                            CreatedUserId = hc.CreatedUserId,
                            HomeWorkName = h.Name,
                            CreatedDate = hc.CreatedDate,
                            CurriculumId = hc.CurriculumId,
                            EndDate = hc.EndDate,
                            NumberRetry = hc.NumberRetry,
                            StartDate = hc.StartDate,
                            HomeWorkId = hc.HomeWorkId,
                            CourseLevel = h.CourseLevel,
                            CourseSkill = h.CourseSkill,
                            Program = p.Name,
                            ProgramId = p.Id,
                            Level = l.Name,
                            LevelId = l.Id,
                            Skill = s.Name,
                            SkillId = s.Id,
                            Subject = p.CategoryParent != null ? p.CategoryParent.Name : null,
                            SubjectId = p.CategoryParent != null ? p.CategoryParent.Id : null,
                        };

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                query = query.Where(p => !string.IsNullOrEmpty(p.HomeWorkName) && p.HomeWorkName.Contains(request.Keyword));
            }

            if (request.CreatedUserIds != null && request.CreatedUserIds.Any())
            {
                query = query.Where(m => request.CreatedUserIds.Contains(m.CreatedUserId));
            }

            if (request.ProgramIds != null && request.ProgramIds.Any())
            {
                query = query.Where(m => m.ProgramId.HasValue && request.ProgramIds.Contains(m.ProgramId.Value));
            }

            if (request.SkillIds != null && request.SkillIds.Any())
            {
                query = query.Where(m => m.SkillId.HasValue && request.SkillIds.Contains(m.SkillId.Value));
            }

            if (request.LevelIds != null && request.LevelIds.Any())
            {
                query = query.Where(m => m.LevelId.HasValue && request.LevelIds.Contains(m.LevelId.Value));
            }

            int totalItem = await query.CountAsync(cancellationToken);

            var lists = await query.ApplySortAndPaging(request)
                                   .AsNoTracking()
                                   .ToListAsync(cancellationToken);

            methodResult.Result = new PagingItemsModel<HomeWorkConfigModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
