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
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchReviewLesssonWithUnitQuery : SearchReviewLesssonWithUnitQueryModel, IRequest<MethodResult<ReviewLessonWithUnitSearchModel>>
    {
    }

    public class SearchReviewLesssonWithUnitQueryHandler : IRequestHandler<SearchReviewLesssonWithUnitQuery, MethodResult<ReviewLessonWithUnitSearchModel>>
    {
        private readonly IUnitRepository _unitRepository;
        private readonly IUserService _userService;
        private readonly ICourseRepository _courseRepository;

        public SearchReviewLesssonWithUnitQueryHandler(IUnitRepository unitRepository, IUserService userService, ICourseRepository courseRepository)
        {
            _unitRepository = unitRepository;
            _userService = userService;
            _courseRepository = courseRepository;
        }

        public async Task<MethodResult<ReviewLessonWithUnitSearchModel>> Handle(SearchReviewLesssonWithUnitQuery request, CancellationToken cancellationToken)
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
                methodResult.Result = null;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var unitQuery = _unitRepository.Queryable.Include(x => x.CourseUnitMockTests)
                                                        .Include(x => x.LessonResults)
                                                        .ThenInclude(x => x.VideoResult)
                                                        .ThenInclude(x => x!.Video)
                                                        .Where(x => x.CourseUnitMockTests.Any(x => x.CourseId == request.CourseId))
                                                        .Select(x => new ReviewLessonWithUnitModel
                                                        {
                                                            Id = x.Id,
                                                            Code = x.Code,
                                                            CreatedDate = x.CreatedDate,
                                                            TeacherIds = x.LessonResults.Select(x => x.VideoResult).Select(x => x!.Video).Select(x => x!.TeacherId).Distinct().ToList(),
                                                            Scores = x.LessonResults.Select(x => x.VideoResult).Where(x => x!.Status == EnumResultStatus.Done).Average(x => x!.NumberOfStars)
                                                        });

            if (request.TeacherId != null)
            {
                unitQuery = unitQuery.Where(x => x.TeacherIds != null && x.TeacherIds!.Contains(request.TeacherId ?? default));
            }

            var scores = await unitQuery.AverageAsync(x => x.Scores, cancellationToken: cancellationToken).ConfigureAwait(false);
            int totalItem = await unitQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await unitQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            var teacherIds = lists.Where(x => x.TeacherIds != null && x.TeacherIds.Count > 0).SelectMany(x => x.TeacherIds!).Distinct().ToList();
            var teacherResults = await _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel { Ids = teacherIds });
            var teachers = teacherResults.Content?.Result;
            if (teachers != null)
            {
                foreach (var item in lists)
                {
                    if (item.TeacherIds != null)
                    {
                        item.TeacherNames = new List<string>();
                        foreach (var teacherId in item.TeacherIds)
                        {
                            var teacher = teachers.FirstOrDefault(x => x.Id == teacherId);
                            item.TeacherNames.Add(teacher?.Human?.FullName!);
                        }
                    }
                }
            }

            methodResult.Result = new ReviewLessonWithUnitSearchModel { Scores = scores, Code = course.Code, PagingItemsModel = new PagingItemsModel<ReviewLessonWithUnitModel>(lists, request, totalItem) };
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
