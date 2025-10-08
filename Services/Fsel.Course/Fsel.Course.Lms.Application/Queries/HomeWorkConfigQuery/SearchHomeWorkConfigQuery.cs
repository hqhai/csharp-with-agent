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
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
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

        public SearchHomeWorkConfigQueryHandler(IHomeWorkConfigRepository homeWorkConfigRepository, IHomeWorkRepository homeWorkRepository)
        {
            _homeWorkConfigRepository = homeWorkConfigRepository;
            _homeWorkRepository = homeWorkRepository;
        }

        public async Task<MethodResult<PagingItemsModel<HomeWorkConfigModel>>> Handle(SearchHomeWorkConfigQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<HomeWorkConfigModel>>();

            var query = from hc in _homeWorkConfigRepository.Queryable
                        join h in _homeWorkRepository.Queryable on hc.HomeWorkId equals h.Id
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
                            CourseSkill = h.CourseSkill
                        };

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                query = query.Where(p => !string.IsNullOrEmpty(p.HomeWorkName) && p.HomeWorkName.Contains(request.Keyword));
            }

            if (request.CourseTypes != null && request.CourseTypes.Any())
            {
                var courseLevels = new List<EnumCourseLevel>();

                request.CourseTypes.ForEach(courseLevel =>
                {
                    courseLevels.AddRange(courseLevel.GetEnumCourseLevels());
                });

                query = query.Where(m => courseLevels.Contains(m.CourseLevel));
            }

            if (request.CourseLevels != null && request.CourseLevels.Any())
            {
                query = query.Where(m => request.CourseLevels.Contains(m.CourseLevel));
            }

            if (request.CourseSkills != null && request.CourseSkills.Any())
            {
                query = query.Where(m => request.CourseSkills.Contains(m.CourseSkill));
            }

            if (request.CreatedUserIds != null && request.CreatedUserIds.Any())
            {
                query = query.Where(m => request.CreatedUserIds.Contains(m.CreatedUserId));
            }

            int totalItem = await query.CountAsync(cancellationToken);

            var lists = await query.ApplySortAndPaging(request).AsNoTracking()
                                   .ToListAsync(cancellationToken);

            methodResult.Result = new PagingItemsModel<HomeWorkConfigModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
