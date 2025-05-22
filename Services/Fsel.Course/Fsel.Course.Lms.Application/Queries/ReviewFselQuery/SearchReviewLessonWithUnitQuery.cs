// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ReviewFselQuery
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
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

    public class SearchReviewLessonWithUnitQuery : SearchReviewLessonWithUnitQueryModel, IRequest<MethodResult<ReviewLessonWithUnitSearchModel>>
    {
    }

    public class SearchReviewLessonWithUnitQueryHandler : IRequestHandler<SearchReviewLessonWithUnitQuery, MethodResult<ReviewLessonWithUnitSearchModel>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IUserService _userService;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly ILessonVideoRepository _lessonVideoRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly IUnitLessonRepository _unitLessonRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly ICourseUnitMockTestRepository _courseUnitMockTestRepository;
        private readonly IVideoRepository _videoRepository;

        public SearchReviewLessonWithUnitQueryHandler(ICourseRepository courseRepository
            , IUserService userService
            , IVideoResultRepository videoResultRepository
            , ILessonVideoRepository lessonVideoRepository
            , ILessonRepository lessonRepository
            , IUnitLessonRepository unitLessonRepository
            , IUnitRepository unitRepository
            , ICourseUnitMockTestRepository courseUnitMockTestRepository
            , IVideoRepository videoRepository)
        {
            _courseRepository = courseRepository;
            _userService = userService;
            _videoResultRepository = videoResultRepository;
            _lessonVideoRepository = lessonVideoRepository;
            _lessonRepository = lessonRepository;
            _unitLessonRepository = unitLessonRepository;
            _unitRepository = unitRepository;
            _courseUnitMockTestRepository = courseUnitMockTestRepository;
            _videoRepository = videoRepository;
        }

        public async Task<MethodResult<ReviewLessonWithUnitSearchModel>> Handle(SearchReviewLessonWithUnitQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ReviewLessonWithUnitSearchModel>();

            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            var course = await _courseRepository.GetByIdAsync(request.CourseId);
            if (course == null)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var unitQuery = from baseQ in _videoResultRepository.Queryable
                            join v in _videoRepository.Queryable on baseQ.VideoId equals v.Id
                            join lv in _lessonVideoRepository.Queryable on v.Id equals lv.VideoId
                            join l in _lessonRepository.Queryable on lv.LessonId equals l.Id
                            join ul in _unitLessonRepository.Queryable on l.Id equals ul.LessonId
                            join u in _unitRepository.Queryable on ul.UnitId equals u.Id
                            join cum in _courseUnitMockTestRepository.Queryable on u.Id equals cum.UnitId
                            join c in _courseRepository.Queryable on cum.CourseId equals c.Id
                            where baseQ.Status == EnumResultStatus.Done && c.Id == request.CourseId
                            select new
                            {
                                Id = u.Id,
                                Code = u.Code,
                                CourseLevel = u.CourseLevel,
                                CreatedDate = u.CreatedDate,
                                Stars = baseQ.NumberOfStars,
                                TeacherId = v.TeacherId,
                            };
            if (request.TeacherId != null)
            {
                unitQuery = unitQuery.Where(x => x.TeacherId == request.TeacherId);
            }
            var query = unitQuery
                .GroupBy(c => new { c.Id, c.Code, c.CreatedDate })
                .Select(group => new ReviewLessonWithUnitModel
                {
                    Id = group.Key.Id,
                    Code = group.Key.Code,
                    CreatedDate = group.Key.CreatedDate,
                    Stars = group.Any() ? group.Average(x => x.Stars) : default,
                    TeacherIds = group.Select(x => x.TeacherId).Distinct().ToList()
                });

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
            var teacherIds = lists.Where(x => x.TeacherIds != null && x.TeacherIds.Count > 0).SelectMany(x => x.TeacherIds!).Distinct().ToList();
            var teacherResults = await _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel { Ids = teacherIds });
            var teachers = teacherResults.Content?.Result;
            if (teachers != null && teachers.Count > 0)
            {
                foreach (var item in lists)
                {
                    item.Stars = NumberHelper.ConvertRound(item.Stars);
                    if (item.TeacherIds != null)
                    {
                        item.TeacherNames = new List<string>();
                        foreach (var teacherId in item.TeacherIds)
                        {
                            var teacher = teachers.FirstOrDefault(x => x.Id == teacherId);
                            if (teacher?.User?.FullName != null)
                            {
                                item.TeacherNames.Add(teacher?.User?.FullName);
                            }
                        }
                    }
                }
            }

            methodResult.Result = new ReviewLessonWithUnitSearchModel { Stars = stars, Code = course.Code, PagingItemsModel = new PagingItemsModel<ReviewLessonWithUnitModel>(lists, request, totalItem) };
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
