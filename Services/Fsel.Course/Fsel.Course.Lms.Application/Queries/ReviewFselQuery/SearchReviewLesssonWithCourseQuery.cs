// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ReviewFselQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.ReviewFsels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class SearchReviewLesssonWithCourseQuery : SearchReviewCourseQueryModel, IRequest<MethodResult<ReviewLesssonWithCourseSearchModel>>
    {
    }

    public class SearchReviewLesssonCourseQueryHandler : IRequestHandler<SearchReviewLesssonWithCourseQuery, MethodResult<ReviewLesssonWithCourseSearchModel>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly ILessonVideoRepository _lessonVideoRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly IUnitLessonRepository _unitLessonRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly ICourseUnitMockTestRepository _courseUnitMockTestRepository;
        private readonly IVideoRepository _videoRepository;

        public SearchReviewLesssonCourseQueryHandler(ICourseRepository courseRepository
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

        public async Task<MethodResult<ReviewLesssonWithCourseSearchModel>> Handle(SearchReviewLesssonWithCourseQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ReviewLesssonWithCourseSearchModel>();

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
                                  Starts = baseQ.NumberOfStars
                              };

            var courseStars = courseQuery
                .GroupBy(c => new { c.Id, c.Code, c.CourseLevel, c.CreatedDate }) // Nhóm dữ liệu theo Id của khóa học
                .Select(group => new ReviewLesssonWithCourseModel
                {
                    Id = group.Key.Id,
                    Code = group.Key.Code,
                    CourseLevel = group.Key.CourseLevel,
                    CreatedDate = group.Key.CreatedDate,
                    Starts = Math.Round(group.Select(x => x.Starts).Average(), 1)
                })
                .ToList();

            courseStars = request.CourseLevel != null ? courseStars.Where(x => x.CourseLevel == request.CourseLevel).ToList() : courseStars;
            var starts = courseStars.Count > 0 ? Math.Round(courseStars.Average(x => x.Starts), 1) : default;

            int totalItem = courseStars.Count;
            var lists = courseStars.OrderBy(x => x.CreatedDate).Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToList();

            methodResult.Result = new ReviewLesssonWithCourseSearchModel { Starts = starts, PagingItemsModel = new PagingItemsModel<ReviewLesssonWithCourseModel>(lists, request, totalItem) };
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
