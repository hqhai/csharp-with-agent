// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Core.Applications.InternalEvents;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class LessonResultInputThenUpdateUnitResultHandler : BaseInternalEventHandler,
        INotificationHandler<EntityChangedEvent<LessonResult>>
    {
        private readonly IUnitRepository _unitRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;

        public LessonResultInputThenUpdateUnitResultHandler(IUnitRepository unitRepository
            , IUnitResultRepository unitResultRepository
            , ILessonResultRepository lessonResultRepository
            , IVideoResultRepository videoResultRepository
            , IClassForumResultRepository classForumResultRepository
            , IHomeWorkResultRepository homeWorkResultRepository
            , ICourseRepository courseRepository
            , FinishOneUnitPublisher finishOneUnitPublisher
            , ICourseResultRepository courseResultRepository
            , FinishOneLevelPassPublisher finishOneLevelPassPublisher
            , IMockTestResultRepository mockTestResultRepository
            , IFinalTestResultRepository finalTestResultRepository
            ) : base(videoResultRepository,
                classForumResultRepository,
                unitResultRepository,
                lessonResultRepository,
                courseResultRepository,
                courseRepository,
                finishOneUnitPublisher,
                finishOneLevelPassPublisher,
                finalTestResultRepository,
                mockTestResultRepository,
                homeWorkResultRepository)
        {
            _unitRepository = unitRepository;
            _lessonResultRepository = lessonResultRepository;
            _mockTestResultRepository = mockTestResultRepository;
        }

        public async Task Handle(EntityChangedEvent<LessonResult> notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            var lessonResult = notification.Data;
            var unit = await _unitRepository.Queryable.Include(x => x.LessonResults.Where(x => x.Status == EnumResultStatus.Done && x.StudentId == lessonResult.StudentId && x.CourseId == lessonResult.CourseId))
                                                    .Include(x => x.UnitLessons)
                                                    .ThenInclude(x => x.Lesson)
                                                    .Include(x => x.UnitSkillMockTests)
                                                    .FirstOrDefaultAsync(x => x.Id == lessonResult.UnitId, cancellationToken);

            if (unit != null && lessonResult.Status == EnumResultStatus.Done)
            {
                if (unit.LessonResults.Count == unit.UnitLessons.Count && unit.UnitSkillMockTests.Count == 0)
                {
                    await UpdateUnit(unit.LessonResults.ToList(), unit, lessonResult.CourseId, lessonResult.StudentId, cancellationToken);
                }
                else if (unit.LessonResults.Count == unit.UnitLessons.Count && unit.UnitSkillMockTests.Count > 0)
                {
                    await UpdateTheNextLesson(unit, lessonResult, cancellationToken);
                }
                else
                {
                    var isCheck = unit.LessonResults.Any(x => x.Status == EnumResultStatus.New);
                    if (!isCheck)
                    {
                        await UpdateTheNextLesson(unit, lessonResult, cancellationToken);
                    }
                }
            }
        }

        public async Task UpdateTheNextLesson(Domain.Entities.Unit? unit, LessonResult lessonResult, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(unit);
            var isCheckDone = true;
            switch (unit.CourseLevel.GetEnumCourseType())
            {
                case EnumCourseType.Ielts:
                    isCheckDone = false;
                    break;

                case EnumCourseType.Academic:
                    isCheckDone = true;
                    break;

                default:
                    break;
            }
            var mockTestId = unit.UnitSkillMockTests.FirstOrDefault()?.MockTestId;
            var displayOrder = unit.UnitLessons.FirstOrDefault(x => x.LessonId == lessonResult.LessonId)!.DisplayOrder;
            var lesson = unit.UnitLessons.FirstOrDefault(x => x.DisplayOrder == displayOrder + 1)?.Lesson;
            if (lesson != null)
            {
                var lessonResultNext = await _lessonResultRepository.Queryable.FirstOrDefaultAsync(x => x.CourseId == lessonResult.CourseId && x.UnitId == lessonResult.UnitId && x.StudentId == lessonResult.StudentId && x.LessonId == lesson.Id, cancellationToken);
                if (lessonResultNext != null)
                {
                    lessonResultNext.Status = EnumResultStatus.New;
                    _lessonResultRepository.Update(lessonResultNext);
                    await _lessonResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                }
            }
            else if (!isCheckDone && mockTestId.HasValue)
            {
                var mockTestResult = await _mockTestResultRepository.Queryable.FirstOrDefaultAsync(x => x.CourseId == lessonResult.CourseId && x.UnitId == lessonResult.UnitId && x.StudentId == lessonResult.StudentId && x.MockTestId == mockTestId.Value, cancellationToken);
                if (mockTestResult != null)
                {
                    mockTestResult.Status = EnumResultStatus.New;
                    _mockTestResultRepository.Update(mockTestResult);
                    await _mockTestResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }
}
