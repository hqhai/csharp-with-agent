// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ReviewFselQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.ReviewFsels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchReviewLesssonWithCourseQuery : SearchReviewCourseQueryModel, IRequest<MethodResult<ReviewLesssonWithCourseSearchModel>>
    {
    }

    public class SearchReviewLesssonCourseQueryHandler : IRequestHandler<SearchReviewLesssonWithCourseQuery, MethodResult<ReviewLesssonWithCourseSearchModel>>
    {
        private readonly ICourseRepository _courseRepository;

        public SearchReviewLesssonCourseQueryHandler(ICourseRepository courseRepository)
        {
            _courseRepository = courseRepository;
        }

        public async Task<MethodResult<ReviewLesssonWithCourseSearchModel>> Handle(SearchReviewLesssonWithCourseQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ReviewLesssonWithCourseSearchModel>();

            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var courseQuery = _courseRepository.Queryable.Include(x => x.LessonResults)
                                                        .ThenInclude(x => x.VideoResult)
                                                        .Select(x => new ReviewLesssonWithCourseModel
                                                        {
                                                            Id = x.Id,
                                                            CreatedDate = x.CreatedDate,
                                                            Code = x.Code,
                                                            CourseLevel = x.CourseLevel,
                                                            Scores = x.LessonResults.Select(x => x.VideoResult).Where(x => x!.Status == EnumResultStatus.Done).Average(x => x!.NumberOfStars)
                                                        });
            if (request.CourseLevel != null)
            {
                courseQuery = courseQuery.Where(x => x.CourseLevel == request.CourseLevel);
            }

            var scores = await courseQuery.AverageAsync(x => x.Scores, cancellationToken: cancellationToken).ConfigureAwait(false);
            int totalItem = await courseQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await courseQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            methodResult.Result = new ReviewLesssonWithCourseSearchModel { Scores = scores, PagingItemsModel = new PagingItemsModel<ReviewLesssonWithCourseModel>(lists, request, totalItem) };
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
