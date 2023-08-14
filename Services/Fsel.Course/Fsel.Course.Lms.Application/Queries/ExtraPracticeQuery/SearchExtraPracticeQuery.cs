// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ExtraPracticeQuery
{
    using System.Linq;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.ExtraPractices;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchExtraPracticeQuery : SearchExtraPracticeLmsQueryModel, IRequest<MethodResult<PagingItemsModel<ExtraPracticeSearchModel>>>
    {
    }

    public class SearchExtraPracticeQueryHandler : IRequestHandler<SearchExtraPracticeQuery, MethodResult<PagingItemsModel<ExtraPracticeSearchModel>>>
    {
        private readonly IExtraPracticeRepository _extraPracticeRepository;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;

        public SearchExtraPracticeQueryHandler(IExtraPracticeRepository extraPracticeRepository
            , IUserService userService
            , AuthContext authContext)
        {
            _extraPracticeRepository = extraPracticeRepository;
            _userService = userService;
            _authContext = authContext;
        }

        public async Task<MethodResult<PagingItemsModel<ExtraPracticeSearchModel>>> Handle(SearchExtraPracticeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<ExtraPracticeSearchModel>>();
            var studentsResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (studentsResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentsResult));
                return methodResult;
            }
            var studentId = studentsResult.Content!.Result!.Id;

            var extraPracticeQuery = _extraPracticeRepository.Queryable
                        .Include(x => x.ExtraPracticeResults)
                        .Include(x => x.LessonExtraPractices)
                            .ThenInclude(x => x.Lesson)
                                .ThenInclude(x => x!.UnitLessons)
                        .Include(x => x.PlacementTest)
                            .ThenInclude(x => x!.PlacementTestSections)
                            .ThenInclude(x => x.SectionGroup)
                        .Include(x => x.MockTest)
                            .ThenInclude(x => x!.MockTestSections)
                            .ThenInclude(x => x.SectionGroup)
                        .Include(x => x.ExtraPracticeChapters.Where(n => !n.IsDeleted))
                            .ThenInclude(x => x.ExtraPracticeExercises.Where(n => !n.IsDeleted))
                                .ThenInclude(x => x.Exercise)
                        .Include(x => x.ExtraPracticeExercises.Where(n => !n.IsDeleted && n.Exercise != null))
                            .ThenInclude(x => x.Exercise)
                        .Include(x => x.Video)
                            .ThenInclude(x => x!.VideoTimeCodes.Where(n => !n.IsDeleted))
                                .ThenInclude(x => x.TimeCodeExercises.Where(n => !n.IsDeleted && n.Exercise != null))
                                    .ThenInclude(x => x.Exercise)
                        .Where(x => x.IsActive)
                        .AsNoTracking()
            .Select(x => new ExtraPracticeSearchModel
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                CreatedDate = x.CreatedDate,
                ImagePath = x.ImagePath,
                Type = x.Type,
                CourseLevel = x.CourseLevel,
                CourseSkills = GetCourseSkills(x),
                UnitId = x.LessonExtraPractices.Select(x => x.Lesson).SelectMany(x => x!.UnitLessons).FirstOrDefault() != null ? x.LessonExtraPractices.Select(x => x.Lesson).SelectMany(x => x!.UnitLessons).FirstOrDefault()!.UnitId : null,
                Percent = x.ExtraPracticeResults.FirstOrDefault(y => y.ExtraPracticeId == x.Id && y.StudentId == studentId) != null ? x.ExtraPracticeResults.FirstOrDefault(y => y.ExtraPracticeId == x.Id && y.StudentId == studentId)!.Percent : null,
                AccessCount = x.ExtraPracticeResults.Count
            });

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                extraPracticeQuery = extraPracticeQuery.Where(m => m.Id.ToString() == request.Keyword || (m.Name ?? string.Empty).Contains(request.Keyword));
            }

            if (request.Types != null && request.Types.Count > 0)
            {
                extraPracticeQuery = extraPracticeQuery.Where(x => request.Types.Contains(x.Type));
            }

            if (request.UnitIds != null && request.UnitIds.Count > 0)
            {
                extraPracticeQuery = extraPracticeQuery.Where(x => request.UnitIds.Contains(x.UnitId ?? Guid.Empty));
            }

            if (request.CourseSkills != null && request.CourseSkills.Count > 0)
            {
                var extraPractices = await extraPracticeQuery.ToListAsync(cancellationToken);
                var extraPracticeModels = extraPractices.Where(x => x.CourseSkills != null && request.CourseSkills.All(cs => x.CourseSkills.Any(x => x == cs))).ToList();
                extraPracticeQuery = extraPracticeQuery.Where(x => extraPracticeModels.Select(y => y.Id).Contains(x.Id));
            }

            if (request.SortFilter != null)
            {
                switch (request.SortFilter)
                {
                    case EnumSortFilter.Newest:
                        extraPracticeQuery = extraPracticeQuery.OrderBy(x => x.CreatedDate);
                        break;

                    case EnumSortFilter.Oldest:
                        extraPracticeQuery = extraPracticeQuery.OrderByDescending(x => x.CreatedDate);
                        break;

                    case EnumSortFilter.MostPopular:
                        extraPracticeQuery = extraPracticeQuery.OrderByDescending(x => x.AccessCount);
                        break;

                    case EnumSortFilter.TrendingNow:
                        DateTime currentDate = DateTime.Now; // Lấy thời gian hiện tại từ hệ thống
                        DayOfWeek currentDayOfWeek = currentDate.DayOfWeek;
                        DateTime startDate = currentDate.AddDays(-(int)currentDayOfWeek); // Ngày đầu tiên của tuần
                        DateTime endDate = startDate.AddDays(6); // Ngày cuối cùng của tuần

                        var objectsInWeek = extraPracticeQuery.Where(obj => obj.CreatedDate >= startDate && obj.CreatedDate <= endDate);
                        extraPracticeQuery = extraPracticeQuery.OrderByDescending(obj => obj.AccessCount);

                        if (objectsInWeek.Any())
                        {
                            extraPracticeQuery = extraPracticeQuery.Except(objectsInWeek).OrderByDescending(obj => obj.AccessCount);
                            extraPracticeQuery = objectsInWeek.OrderByDescending(obj => obj.AccessCount).Concat(extraPracticeQuery);
                        }
                        break;

                    default:
                        break;
                }
            }
            if (request.Progresses != null && request.Progresses.Count != 0)
            {
                extraPracticeQuery = extraPracticeQuery.Where(x =>
                    (request.Progresses.Contains(EnumExtraPracticeProgress.UnOpened) && x.Percent == 0) ||
                    (request.Progresses.Contains(EnumExtraPracticeProgress.InProgress) && x.Percent <= 100 && x.Percent >= 0) ||
                    (request.Progresses.Contains(EnumExtraPracticeProgress.Completed) && x.Percent == 100)
                );
            }

            int totalItem = await extraPracticeQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await extraPracticeQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            methodResult.Result = new PagingItemsModel<ExtraPracticeSearchModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private static IList<EnumCourseSkill>? GetCourseSkills(ExtraPractice extraPractice)
        {
            ArgumentNullException.ThrowIfNull(extraPractice);
            var courseSkills = new List<EnumCourseSkill>();
            switch (extraPractice.Type)
            {
                case EnumExtraPracticeType.Book:
                    courseSkills = extraPractice.ExtraPracticeChapters.SelectMany(x => x.ExtraPracticeExercises).Select(x => x.Exercise).Distinct().Select(x => x!.CourseSkill).ToList();
                    break;

                case EnumExtraPracticeType.VideoEmbed:
                    courseSkills = extraPractice.ExtraPracticeExercises.Select(x => x.Exercise).Select(x => x!.CourseSkill).Distinct().ToList();
                    break;

                case EnumExtraPracticeType.InteractiveVideo:
                    courseSkills = extraPractice.Video != null ? extraPractice.Video.VideoTimeCodes.SelectMany(x => x!.TimeCodeExercises).Select(x => x.Exercise).Select(x => x!.CourseSkill).Distinct().ToList() : null;
                    break;

                case EnumExtraPracticeType.MockTest:
                    if (extraPractice.MockTestId != null)
                    {
                        courseSkills = extraPractice.MockTest?.MockTestSections.Select(x => x.SectionGroup).Select(x => x!.CourseSkill).Distinct().ToList();
                    }
                    else
                    {
                        courseSkills = extraPractice.PlacementTest?.PlacementTestSections.Select(x => x.SectionGroup).Select(x => x!.CourseSkill).Distinct().ToList();
                    }
                    break;

                case EnumExtraPracticeType.Exercise:
                    courseSkills = extraPractice.ExtraPracticeExercises.Select(x => x.Exercise).Select(x => x!.CourseSkill).Distinct().ToList();
                    break;

                case EnumExtraPracticeType.Articles:
                    courseSkills = null;
                    break;
            }
            return courseSkills;
        }
    }
}
