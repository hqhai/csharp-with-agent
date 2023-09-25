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
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchReviewLessonWithCourseQuery : SearchReviewCourseQueryModel, IRequest<MethodResult<ReviewLessonWithCourseSearchModel>>
    {
    }

    public class SearchReviewLessonCourseQueryHandler : IRequestHandler<SearchReviewLessonWithCourseQuery, MethodResult<ReviewLessonWithCourseSearchModel>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly ILessonVideoRepository _lessonVideoRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly IUnitLessonRepository _unitLessonRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly ICourseUnitMockTestRepository _courseUnitMockTestRepository;
        private readonly IVideoRepository _videoRepository;

        public SearchReviewLessonCourseQueryHandler(ICourseRepository courseRepository
            , IVideoResultRepository videoResultRepository
            , ILessonVideoRepository lessonVideoRepository
            , ILessonRepository lessonRepository
            , IUnitLessonRepository unitLessonRepository
            , IUnitRepository unitRepository
            , ICourseUnitMockTestRepository courseUnitMockTestRepository
            , IVideoRepository videoRepository)
        {
            _courseRepository = courseRepository;
            _videoResultRepository = videoResultRepository;
            _lessonVideoRepository = lessonVideoRepository;
            _lessonRepository = lessonRepository;
            _unitLessonRepository = unitLessonRepository;
            _unitRepository = unitRepository;
            _courseUnitMockTestRepository = courseUnitMockTestRepository;
            _videoRepository = videoRepository;
        }

        public async Task<MethodResult<ReviewLessonWithCourseSearchModel>> Handle(SearchReviewLessonWithCourseQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ReviewLessonWithCourseSearchModel>();

            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            var courseQuery = from baseQ in _videoResultRepository.Queryable
                              join v in _videoRepository.Queryable on baseQ.VideoId equals v.Id
                              join lv in _lessonVideoRepository.Queryable on v.Id equals lv.VideoId
                              join l in _lessonRepository.Queryable on lv.LessonId equals l.Id
                              join ul in _unitLessonRepository.Queryable on l.Id equals ul.LessonId
                              join u in _unitRepository.Queryable on ul.UnitId equals u.Id
                              join cum in _courseUnitMockTestRepository.Queryable on u.Id equals cum.UnitId
                              join c in _courseRepository.Queryable on cum.CourseId equals c.Id
                              where baseQ.Status == EnumResultStatus.Done
                              select new
                              {
                                  Id = c.Id,
                                  Code = c.Code,
                                  CourseLevel = c.CourseLevel,
                                  CreatedDate = c.CreatedDate,
                                  Stars = baseQ.NumberOfStars
                              };

            var query = courseQuery
                .GroupBy(c => new { c.Id, c.Code, c.CourseLevel, c.CreatedDate })
                .Select(group => new ReviewLessonWithCourseModel
                {
                    Id = group.Key.Id,
                    Code = group.Key.Code,
                    CourseLevel = group.Key.CourseLevel,
                    CreatedDate = group.Key.CreatedDate,
                    Stars = group.Average(x => x.Stars)
                });

            if (request.NumberOfStars != null)
            {
                query = query.Where(x => x.Stars >= request.NumberOfStars && x.Stars < request.NumberOfStars + 0.5);
            }

            //var result = await query.ToListAsync(cancellationToken);
            var stars = NumberHelper.ConvertDoubleDecimal(await query.AverageAsync(x => x.Stars, cancellationToken));
            int totalItem = await query.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await query
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            foreach (var item in lists)
            {
                item.Stars = NumberHelper.ConvertDoubleDecimal(item.Stars);
            }

            methodResult.Result = new ReviewLessonWithCourseSearchModel { Stars = stars, PagingItemsModel = new PagingItemsModel<ReviewLessonWithCourseModel>(lists, request, totalItem) };
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
