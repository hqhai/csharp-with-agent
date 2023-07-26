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
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchReviewLesssonWithLessonQuery : SearchReviewLesssonWithLessonQueryModel, IRequest<MethodResult<ReviewLessonWithLessonSearchModel>>
    {
    }

    public class SearchReviewLesssonWithLessonQueryHandler : IRequestHandler<SearchReviewLesssonWithLessonQuery, MethodResult<ReviewLessonWithLessonSearchModel>>
    {
        private readonly IUnitRepository _unitRepository;
        private readonly IUserService _userService;
        private readonly ILessonRepository _lessonRepository;

        public SearchReviewLesssonWithLessonQueryHandler(IUnitRepository unitRepository, IUserService userService, ILessonRepository lessonRepository)
        {
            _unitRepository = unitRepository;
            _userService = userService;
            _lessonRepository = lessonRepository;
        }

        public async Task<MethodResult<ReviewLessonWithLessonSearchModel>> Handle(SearchReviewLesssonWithLessonQuery request, CancellationToken cancellationToken)
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
                methodResult.Result = null;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var lessonQuery = _lessonRepository.Queryable.Include(x => x.LessonVideos)
                                                        .ThenInclude(x => x.Video)
                                                        .Include(x => x.UnitLessons)
                                                        .Include(x => x.LessonResults)
                                                        .ThenInclude(x => x.VideoResult)
                                                        .Where(x => x.UnitLessons.Any(x => x.UnitId == request.UnitId))
                                                        .Select(x => new ReviewLessonWithLessonModel
                                                        {
                                                            Id = x.Id,
                                                            Name = x.Name,
                                                            CreatedDate = x.CreatedDate,
                                                            TeacherId = x.LessonVideos.FirstOrDefault(x => x.LessonId == x.Id)!.Video!.TeacherId,
                                                            Scores = x.LessonResults.Select(x => x.VideoResult).Where(x => x!.Status == EnumResultStatus.Done).Average(x => x!.NumberOfStars)
                                                        });

            if (request.TeacherId != null)
            {
                lessonQuery = lessonQuery.Where(x => x.TeacherId == request.TeacherId);
            }

            var scores = await lessonQuery.AverageAsync(x => x.Scores, cancellationToken: cancellationToken).ConfigureAwait(false);
            int totalItem = await lessonQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await lessonQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            var teacherIds = lists.Select(x => x.TeacherId).Distinct().ToList();
            var teacherResults = await _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel { Ids = teacherIds });
            var teachers = teacherResults.Content?.Result;
            if (teachers != null)
            {
                foreach (var item in lists)
                {
                    var teacher = teachers.FirstOrDefault(x => x.Id == request.TeacherId);
                    item.FullName = teacher?.Human?.FullName;
                }
            }

            methodResult.Result = new ReviewLessonWithLessonSearchModel { Scores = scores, Code = unit.Code, PagingItemsModel = new PagingItemsModel<ReviewLessonWithLessonModel>(lists, request, totalItem) };
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
