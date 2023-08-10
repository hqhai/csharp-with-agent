// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ProgessQuery
{
    using System.Linq;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetProgessMenuQuery : IRequest<MethodResult<ProgessMenuModel>>
    {
        public Guid CourseId { get; set; }
    }

    public class GetProgessMenuQueryHandler : IRequestHandler<GetProgessMenuQuery, MethodResult<ProgessMenuModel>>
    {
        private readonly AuthContext _authContext;
        private readonly IClassForumRepository _classForumRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly ILessonExtraPracticeRepository _lessonExtraPracticeRepository;
        private readonly ISystemService _systemService;
        private readonly IUserService _userService;

        public GetProgessMenuQueryHandler(AuthContext authContext
            , IClassForumRepository classForumRepository
            , IUnitRepository unitRepository
            , ILessonExtraPracticeRepository lessonExtraPracticeRepository
            , ISystemService systemService
            , IUserService userService)
        {
            _authContext = authContext;
            _classForumRepository = classForumRepository;
            _unitRepository = unitRepository;
            _lessonExtraPracticeRepository = lessonExtraPracticeRepository;
            _systemService = systemService;
            _userService = userService;
        }

        public async Task<MethodResult<ProgessMenuModel>> Handle(GetProgessMenuQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ProgessMenuModel> methodResult = new MethodResult<ProgessMenuModel>();
            ProgessMenuModel progessMenu = new ProgessMenuModel();
            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentResult));
                return methodResult;
            }
            var studentId = studentResult?.Content?.Result?.Id;

            var units = await _unitRepository.Queryable.Include(x => x.UnitResults.Where(x => x.CourseId == request.CourseId && x.StudentId == studentId))
                                                        .Include(x => x.UnitLessons)
                                                        .ToListAsync(cancellationToken);
            if (units == null || units.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(units));
                return methodResult;
            }
            var numberOfUnitDone = units.SelectMany(x => x.UnitResults).Where(x => x.Status == EnumResultStatus.Done).Count();
            var lessonIds = units.SelectMany(x => x.UnitLessons).Select(x => x.LessonId).ToList();

            var lessonExtraPractices = await _lessonExtraPracticeRepository.Queryable.Include(x => x.ExtraPractice)
                .ThenInclude(x => x!.ExtraPracticeResults)
                .Where(x => lessonIds.Contains(x.LessonId))
                .ToListAsync(cancellationToken);
            var numberOfPracticesDone = lessonExtraPractices.Select(x => x.ExtraPractice)
                                                            .Where(x => x!.ExtraPracticeResults.Count > 0)
                                                            .SelectMany(x => x!.ExtraPracticeResults)
                                                            .Where(x => x.Status == EnumResultStatus.Done)
                                                            .Count();
            var classForums = await _classForumRepository.Queryable.Include(x => x.ClassForumResults.Where(x => x.StudentId == studentId))
                                                                   .Where(x => lessonIds.Contains(x.LessonId))
                                                                   .ToListAsync(cancellationToken);
            var numberOfPostsCreated = classForums.SelectMany(x => x.ClassForumResults)
                                                    .Where(x => x.Status == EnumClassForumResultStatus.PendingForGrading || x.Status == EnumClassForumResultStatus.Graded)
                                                    .Count();
            var logActionResults = await _systemService.GetLogActionsByUserId(_authContext.CurrentUserId);
            if (!logActionResults.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(logActionResults));
                return methodResult;
            }
            var (numberOfDaysStreak, isDaysStreakIncrease) = (0, true);
            var logActions = logActionResults?.Content?.Result;
            if (logActions != null)
            {
                progessMenu.NumberOfDaysStreak = logActions.NumberOfDaysStreak;
                progessMenu.IsDaysStreakIncrease = logActions.IsDaysStreakIncrease;
            }

            progessMenu.NumberOfUnitDone = numberOfUnitDone;
            progessMenu.NumberOfPostsCreated = numberOfPostsCreated;
            progessMenu.NumberOfPracticesDone = numberOfPracticesDone;
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = progessMenu;
            return methodResult;
        }
    }
}
