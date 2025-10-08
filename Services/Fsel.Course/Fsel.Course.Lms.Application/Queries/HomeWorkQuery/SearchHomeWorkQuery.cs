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
    using Fsel.Course.Domain.Models.QueryModels.HomeWorks;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchHomeWorkQuery : SearchHomeWorkModel, IRequest<MethodResult<PagingItemsModel<HomeWorkSearchModel>>>
    {
    }

    public class SearchHomeWorkQueryHandler : IRequestHandler<SearchHomeWorkQuery, MethodResult<PagingItemsModel<HomeWorkSearchModel>>>
    {
        private readonly IHomeWorkRepository _homeWorkRepository;

        public SearchHomeWorkQueryHandler(IHomeWorkRepository homeWorkRepository)
        {
            _homeWorkRepository = homeWorkRepository;
        }

        public async Task<MethodResult<PagingItemsModel<HomeWorkSearchModel>>> Handle(SearchHomeWorkQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PagingItemsModel<HomeWorkSearchModel>> methodResult = new MethodResult<PagingItemsModel<HomeWorkSearchModel>>();

            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var query = _homeWorkRepository.Queryable.Where(p => !p.IsArchive && p.Type == EnumHomeWorkType.HomeworkExtra)
                                   .Select(x => new HomeWorkSearchModel
                                   {
                                       Id = x.Id,
                                       Code = x.Code,
                                       Name = x.Name,
                                       CreatedFullName = x.CreatedFullName,
                                       CreatedDate = x.CreatedDate,
                                       IsActive = x.LessonHomeWorks.Any(),
                                       CourseLevel = x.CourseLevel,
                                       CourseSkill = x.CourseSkill,
                                       CreatedUserId = x.CreatedUserId,
                                   });

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
