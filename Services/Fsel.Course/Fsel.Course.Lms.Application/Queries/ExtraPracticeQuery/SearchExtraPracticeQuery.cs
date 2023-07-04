// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ExtraPracticeQuery
{
    using System.Linq;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.ExtraPractices;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchExtraPracticeQuery : SearchExtraPracticeLmsQueryModel, IRequest<MethodResult<PagingItemsModel<ExtraPracticeModel>>>
    {
    }

    public class SearchExtraPracticeQueryHandler : IRequestHandler<SearchExtraPracticeQuery, MethodResult<PagingItemsModel<ExtraPracticeModel>>>
    {
        private readonly IExtraPracticeRepository _extraPracticeRepository;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;

        public SearchExtraPracticeQueryHandler(IExtraPracticeRepository extraPracticeRepository
            , IMapper mapper
            , IUserService userService
            , AuthContext authContext)
        {
            _extraPracticeRepository = extraPracticeRepository;
            _mapper = mapper;
            _userService = userService;
            _authContext = authContext;
        }

        public async Task<MethodResult<PagingItemsModel<ExtraPracticeModel>>> Handle(SearchExtraPracticeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<ExtraPracticeModel>>();
            var studentsResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (studentsResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumVideoErrorCode.UserNotExist));
                return methodResult;
            }
            var studentId = studentsResult.Content!.Result!.Id;

            var extraPracticeQuery = _extraPracticeRepository.Queryable
                        .Include(x => x.PlacementTest)
                        .Include(x => x.LessonExtraPractices.Where(n => !n.IsDeleted && n.Lesson != null))
                            .ThenInclude(x => x.Lesson)
                            .ThenInclude(x => x!.UnitLessons.Where(n => !n.IsDeleted))
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
            .Select(x => new ExtraPracticeModel
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                CreatedDate = x.CreatedDate,
                Type = x.Type,
                BookFilePath = x.BookFilePath,
                BookCoverPath = x.BookCoverPath,
                BookBackgroundPath = x.BookBackgroundPath,
                CourseLevel = x.CourseLevel,
                CourseSkills = x.ExtraPracticeChapters != null ? x.ExtraPracticeChapters.Where(n => !n.IsDeleted)
                    .SelectMany(e => e.ExtraPracticeExercises.Where(n => n.Exercise != null && !n.IsDeleted))
                        .Select(e => e.Exercise!.CourseSkill).Distinct().ToList()
                    : x.ExtraPracticeExercises != null ? x.ExtraPracticeExercises.Where(n => n.Exercise != null && !n.IsDeleted)
                        .Select(e => e.Exercise!.CourseSkill).Distinct().ToList()
                    : x.Video != null ? x.Video.VideoTimeCodes.SelectMany(v => v.TimeCodeExercises.Where(n => n.Exercise != null && !n.IsDeleted))
                        .Select(v => v.Exercise!.CourseSkill).Distinct().ToList()
                    : x.PlacementTest != null ? x.PlacementTest.PlacementTestSections.Where(n => n.SectionGroup != null && !n.IsDeleted)
                        .Select(p => p.SectionGroup!.CourseSkill).Distinct().ToList()
                    : null,
                PlacementTest = x.PlacementTest != null ? _mapper.Map<PlacementTestModel>(x.PlacementTest) : null,
                MockTest = x.MockTest != null ? _mapper.Map<MockTestModel>(x.MockTest) : null,
                UnitId = x.LessonExtraPractices.Where(n => !n.IsDeleted && n.Lesson != null)
                    .Select(x => x.Lesson)
                    .SelectMany(x => x!.UnitLessons)
                    .Select(x => x.UnitId)
                    .FirstOrDefault(),
                Percent = x.ExtraPracticeResults.FirstOrDefault(y => y != null && y.ExtraPracticeId == x.Id && y.StudentId == studentId)!.Percent,
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
                extraPracticeQuery = extraPracticeQuery.Where(x => x.CourseSkills != null && request.CourseSkills.All(cs => x.CourseSkills.Contains(cs)));
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
                    (request.Progresses.Contains(EnumExtraPracticeProgress.Unopened) && x.Percent == 0) ||
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

            methodResult.Result = new PagingItemsModel<ExtraPracticeModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
