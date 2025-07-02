// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ReviewFselQuery
{
    using System.Linq;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.ReviewFsels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchReviewLessonWithLessonQuery : SearchReviewLessonWithLessonQueryModel, IRequest<MethodResult<ReviewLessonWithLessonSearchModel>>
    {
    }

    public class SearchReviewLessonWithLessonQueryHandler : IRequestHandler<SearchReviewLessonWithLessonQuery, MethodResult<ReviewLessonWithLessonSearchModel>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IUserService _userService;
        private readonly ILessonVideoRepository _lessonVideoRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly IUnitLessonRepository _unitLessonRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly ICourseUnitMockTestRepository _courseUnitMockTestRepository;
        private readonly IVideoRepository _videoRepository;

        public SearchReviewLessonWithLessonQueryHandler(ICourseRepository courseRepository
            , IVideoResultRepository videoResultRepository
            , IUserService userService
            , ILessonVideoRepository lessonVideoRepository
            , ILessonRepository lessonRepository
            , IUnitLessonRepository unitLessonRepository
            , IUnitRepository unitRepository
            , ICourseUnitMockTestRepository courseUnitMockTestRepository
            , IVideoRepository videoRepository)
        {
            _courseRepository = courseRepository;
            _videoResultRepository = videoResultRepository;
            _userService = userService;
            _lessonVideoRepository = lessonVideoRepository;
            _lessonRepository = lessonRepository;
            _unitLessonRepository = unitLessonRepository;
            _unitRepository = unitRepository;
            _courseUnitMockTestRepository = courseUnitMockTestRepository;
            _videoRepository = videoRepository;
        }

        public async Task<MethodResult<ReviewLessonWithLessonSearchModel>> Handle(SearchReviewLessonWithLessonQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ReviewLessonWithLessonSearchModel>();

            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            var unit = await _unitRepository.GetByIdAsync(request.UnitId);
            if (unit == null)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var lessonQuery = from baseQ in _videoResultRepository.Queryable
                              join v in _videoRepository.Queryable on baseQ.VideoId equals v.Id
                              join lv in _lessonVideoRepository.Queryable on v.Id equals lv.VideoId
                              join l in _lessonRepository.Queryable on lv.LessonId equals l.Id
                              join ul in _unitLessonRepository.Queryable on l.Id equals ul.LessonId
                              join u in _unitRepository.Queryable on ul.UnitId equals u.Id
                              join cum in _courseUnitMockTestRepository.Queryable on u.Id equals cum.UnitId
                              join c in _courseRepository.Queryable on cum.CourseId equals c.Id
                              where baseQ.Status == EnumResultStatus.Done && c.Id == request.CourseId && u.Id == request.UnitId
                              select new
                              {
                                  Id = l.Id,
                                  Name = l.Name,
                                  CourseLevel = l.CourseLevel,
                                  CreatedDate = l.CreatedDate,
                                  Stars = baseQ.NumberOfStars,
                                  TeacherId = v.TeacherId,
                              };
            var query = lessonQuery
                .GroupBy(c => new { c.Id, c.Name, c.CreatedDate, c.TeacherId }) // Nhóm dữ liệu theo Id của khóa học
                .Select(group => new ReviewLessonWithLessonModel
                {
                    Id = group.Key.Id,
                    Name = group.Key.Name,
                    CreatedDate = group.Key.CreatedDate,
                    Stars = group.Any() ? group.Average(x => x.Stars) : default,
                    TeacherId = group.Key.TeacherId,
                    TotalStart = group.Sum(x => x.Stars),
                    TotalResult = group.Select(x => x.Stars).Count()
                });
            if (request.TeacherId != null)
            {
                query = query.Where(x => x.TeacherId == request.TeacherId);
            }

            if (request.NumberOfStars != null)
            {
                query = query.Where(x => x.Stars + 0.5 >= request.NumberOfStars && x.Stars < request.NumberOfStars + 0.5);
            }
            var result = await query.ToListAsync(cancellationToken);
            var stars = result.Any() ? NumberHelper.ConvertRound(result.Average(x => x.Stars)) : default;
            int totalItem = await query.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await query
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            var teacherResults = await _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel { Ids = lists.Select(x => x.TeacherId).Distinct().ToList() });
            var teachers = teacherResults.Content?.Result;
            if (teachers != null && teachers.Count > 0)
            {
                foreach (var item in lists)
                {
                    item.Stars = NumberHelper.ConvertRound(item.Stars);
                    var teacher = teachers.FirstOrDefault(x => x.Id == item.TeacherId);
                    item.FullName = teacher?.User?.FullName;
                }
            }

            methodResult.Result = new ReviewLessonWithLessonSearchModel { Stars = stars, Code = unit.Code, PagingItemsModel = new PagingItemsModel<ReviewLessonWithLessonModel>(lists, request, totalItem) };
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
