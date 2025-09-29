// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.CourseQuery.V1i1
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.Courses;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchCourseQuery : SearchCourseQueryModel, IRequest<MethodResult<PagingItemsModel<CourseSearchModel>>>
    {
    }

    public class SearchCourseQueryHandler : IRequestHandler<SearchCourseQuery, MethodResult<PagingItemsModel<CourseSearchModel>>>
    {
        private readonly ICourseRepository _courseRepository;

        public SearchCourseQueryHandler(ICourseRepository courseRepository)
        {
            _courseRepository = courseRepository;
        }

        public async Task<MethodResult<PagingItemsModel<CourseSearchModel>>> Handle(SearchCourseQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<CourseSearchModel>>();

            var courseQuery = _courseRepository.Queryable.Where(p => !p.IsArchive && !p.ParentCourseId.HasValue && p.Status == EnumCourseStatus.Active)
                              .Select(course => new CourseSearchModel
                              {
                                  Id = course.Id,
                                  Name = course.Name,
                                  Code = course.Code,
                                  InstructionContent = course.InstructionContent,
                                  Status = course.Status,
                                  CourseLevel = course.CourseLevel,
                                  CreatedDate = course.CreatedDate,
                                  CreatedUserId = course.CreatedUserId,
                                  CreatedFullName = course.CreatedFullName,
                                  UpdatedDate = course.UpdatedDate,
                                  UpdatedUserId = course.UpdatedUserId,
                                  UpdatedFullName = course.UpdatedFullName,
                                  CourseType = course.CourseType,
                              });

            request.Keyword = request.Keyword?.Trim().ToLower(System.Globalization.CultureInfo.CurrentCulture);
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                courseQuery = courseQuery.Where(m => !string.IsNullOrEmpty(m.Code) && m.Code.Contains(request.Keyword) || !string.IsNullOrEmpty(m.Name) && m.Name.Contains(request.Keyword));
            }

            if (request.CourseLevel.HasValue)
            {
                courseQuery = courseQuery.Where(m => m.CourseLevel == request.CourseLevel);
            }

            if (request.CourseType.HasValue)
            {
                courseQuery = courseQuery.Where(m => m.CourseType == request.CourseType);
            }

            int totalItem = await courseQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await courseQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            methodResult.Result = new PagingItemsModel<CourseSearchModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
