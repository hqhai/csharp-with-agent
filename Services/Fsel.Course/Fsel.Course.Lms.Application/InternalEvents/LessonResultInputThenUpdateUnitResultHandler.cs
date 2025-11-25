// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents
{
    using Amazon.Runtime.Internal.Util;
    using Fsel.Core.Applications.InternalEvents;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Infrastructure.ValueSettings;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.OrderServices;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Unit = Course.Domain.Entities.Unit;

    public class LessonResultInputThenUpdateUnitResultHandler : BaseInternalUnitResultEventHandler,
        INotificationHandler<EntityChangedEvent<LessonResult>>
    {
        private readonly ILessonResultRepository _lessonResultRepository;

        public LessonResultInputThenUpdateUnitResultHandler(ISystemService systemService, ILessonResultRepository lessonResultRepository, AppSetting appSetting, ICourseUnitMockTestRepository courseUnitMockTestRepository, IMediator mediator, IUserService userService, SaveUserCourseSettingPublisher saveUserCourseSettingPublisher, IVideoResultRepository videoResultRepository, IClassForumResultRepository classForumResultRepository, IUnitResultRepository unitResultRepository, ICourseResultRepository courseResultRepository, ICourseRepository courseRepository, IUnitRepository unitRepository, IFinalTestResultRepository finalTestResultRepository, IMockTestResultRepository mockTestResultRepository, IHomeWorkResultRepository homeWorkResultRepository, QuestBoardPublisher questBoardPublisher, IOrderService orderService, ILessonNoteRepository lessonNoteRepository, ILogger<BaseInternalUnitResultEventHandler> logger, NotificationMessagePublisher notificationMessagePublisher, SaveUserSurveyAssignmentPublisher saveUserSurveyAssignmentPublisher) : base(systemService, appSetting, courseUnitMockTestRepository, mediator, userService, saveUserCourseSettingPublisher, videoResultRepository, classForumResultRepository, unitResultRepository, courseResultRepository, courseRepository, unitRepository, finalTestResultRepository, mockTestResultRepository, homeWorkResultRepository, questBoardPublisher, lessonResultRepository, orderService, lessonNoteRepository, logger, notificationMessagePublisher, saveUserSurveyAssignmentPublisher)
        {
            _lessonResultRepository = lessonResultRepository;
        }

        public async Task Handle(EntityChangedEvent<LessonResult> notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            var lessonResult = notification.Data;

            try
            {
                var unit = await _unitRepository.Queryable.Include(x => x.UnitLessons)
                                                        .ThenInclude(x => x.Lesson)
                                                        .Include(x => x.UnitSkillMockTests)
                                                        .FirstOrDefaultAsync(x => x.Id == lessonResult.UnitId, cancellationToken);
                if (unit == null || lessonResult.Status != EnumResultStatus.Done)
                {
                    return;
                }
                var lessonResults = await _lessonResultRepository.Queryable.Where(x => x.Status == EnumResultStatus.Done && (!(unit != null) || x.UnitId == unit.Id) && x.StudentId == lessonResult.StudentId && x.CourseId == lessonResult.CourseId).ToListAsync(cancellationToken);
                var lessonResultIds = lessonResults.Select(x => x.Id).ToList();
                if (lessonResults.Count == unit.UnitLessons.Count && !unit.UnitSkillMockTests.Any())
                {
                    await UpdateUnitResultAsync(lessonResults, unit, lessonResult.CourseId, lessonResult.StudentId, true, cancellationToken);
                }
                else if (lessonResults.Count == unit.UnitLessons.Count && unit.UnitSkillMockTests.Any())
                {
                    var isUnitDone = await _mockTestResultRepository.Queryable.AnyAsync(x => x.Status == EnumResultStatus.Done && x.UnitId == unit.Id && x.StudentId == lessonResult.StudentId && x.CourseId == lessonResult.CourseId, cancellationToken);
                    await UpdateTheNextLessonAsync(unit, lessonResult, cancellationToken);
                    await UpdateUnitResultAsync(lessonResults, unit, lessonResult.CourseId, lessonResult.StudentId, isUnitDone, cancellationToken);
                }
                else
                {
                    var isCheck = lessonResults.Any(x => x.Status == EnumResultStatus.New);
                    if (!isCheck)
                    {
                        await UpdateUnitResultAsync(lessonResults, unit, lessonResult.CourseId, lessonResult.StudentId, false, cancellationToken);
                        await UpdateTheNextLessonAsync(unit, lessonResult, cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Log Trigger LessonResult : {ex.Message} ");
            }
        }

        private async Task UpdateTheNextLessonAsync(Unit? unit, LessonResult lessonResult, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(unit);
            var mockTestId = unit.UnitSkillMockTests.FirstOrDefault()?.MockTestId;
            var displayOrder = unit.UnitLessons.FirstOrDefault(x => x.LessonId == lessonResult.LessonId)!.DisplayOrder;
            var lesson = unit.UnitLessons.FirstOrDefault(x => x.DisplayOrder == displayOrder + 1)?.Lesson;
            if (lesson != null)
            {
                var lessonResultNext = await _lessonResultRepository.Queryable.FirstOrDefaultAsync(x => x.CourseId == lessonResult.CourseId && x.UnitId == lessonResult.UnitId && x.StudentId == lessonResult.StudentId && x.LessonId == lesson.Id, cancellationToken);
                if (lessonResultNext != null && lessonResultNext.Status == EnumResultStatus.Unfinished)
                {
                    lessonResultNext.NewDate = DateTime.UtcNow;
                    lessonResultNext.Status = EnumResultStatus.New;
                    await _lessonResultRepository.BulkUpdateList(new List<LessonResult> { lessonResultNext }, bulk =>
                    {
                        bulk.IgnoreOnUpdateExpression = c => new { c.CourseId, c.StudentId, c.UnitId, c.LessonId };
                    });
                }
            }
            else if (mockTestId.HasValue)
            {
                var mockTestResult = await _mockTestResultRepository.Queryable.FirstOrDefaultAsync(x => x.CourseId == lessonResult.CourseId && x.UnitId == lessonResult.UnitId && x.StudentId == lessonResult.StudentId && x.MockTestId == mockTestId.Value, cancellationToken);
                if (mockTestResult != null && mockTestResult.Status == EnumResultStatus.Unfinished)
                {
                    mockTestResult.NewDate = DateTime.UtcNow;
                    mockTestResult.Status = EnumResultStatus.New;
                    await _mockTestResultRepository.BulkUpdateList(new List<MockTestResult> { mockTestResult }, bulk =>
                    {
                        bulk.IgnoreOnUpdateExpression = c => new { c.CourseId, c.StudentId, c.UnitId, c.MockTestId };
                    });
                }
            }
        }
    }
}
