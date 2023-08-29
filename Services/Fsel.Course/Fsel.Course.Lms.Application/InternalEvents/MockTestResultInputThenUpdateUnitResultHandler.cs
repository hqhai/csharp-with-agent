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
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class MockTestResultInputThenUpdateUnitResultHandler : BaseInternalEventHandler,
        INotificationHandler<EntityChangedEvent<MockTestResult>>
    {
        private readonly IUnitRepository _unitRepository;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly IMockTestRepository _mockTestRepository;

        public MockTestResultInputThenUpdateUnitResultHandler(IUnitRepository unitRepository
            , ILessonResultRepository lessonResultRepository
            , IVideoResultRepository videoResultRepository
            , IClassForumResultRepository classForumResultRepository
            , IHomeWorkResultRepository homeWorkResultRepository
            , ICourseRepository courseRepository
            , ICourseResultRepository courseResultRepository
            , IMockTestRepository mockTestRepository
            , IUnitResultRepository unitResultRepository
            , IMockTestResultRepository mockTestResultRepository
            , FinishOneLevelPassPublisher finishOneLevelPassPublisher
            , IFinalTestResultRepository finalTestResultRepository
            ) : base(videoResultRepository,
                classForumResultRepository,
                unitResultRepository,
                lessonResultRepository,
                courseResultRepository,
                courseRepository,
                finishOneLevelPassPublisher,
                finalTestResultRepository,
                mockTestResultRepository,
                homeWorkResultRepository)
        {
            _unitRepository = unitRepository;
            _unitResultRepository = unitResultRepository;
            _mockTestRepository = mockTestRepository;
        }

        public async Task Handle(EntityChangedEvent<MockTestResult> notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            var mockTestResult = notification.Data;
            var mockTest = await _mockTestRepository.GetByIdAsync(mockTestResult.MockTestId);

            if (mockTest != null && mockTest.MockTestType == EnumMockTestType.SkillMockTest && mockTestResult.Status == EnumResultStatus.Done)
            {
                var unit = await _unitRepository.Queryable.Include(x => x.UnitLessons)
                                                   .Include(x => x.LessonResults.Where(x => x.Status == EnumResultStatus.Done && x.StudentId == mockTestResult.StudentId && x.CourseId == mockTestResult.CourseId))
                                                   .FirstOrDefaultAsync(x => x.Id == mockTestResult.UnitId, cancellationToken);
                if (unit != null)
                {
                    var unitResult = await _unitResultRepository.Queryable.FirstOrDefaultAsync(x => x.UnitId == mockTestResult.UnitId && x.StudentId == mockTestResult.StudentId && x.CourseId == mockTestResult.CourseId, cancellationToken);
                    if (unitResult != null && unit.LessonResults.Count == unit.UnitLessons.Count)
                    {
                        await UpdateUnit(unit.LessonResults.ToList(), unit, mockTestResult.CourseId, mockTestResult.StudentId, cancellationToken);
                    }
                }
            }
            else if (mockTest != null && mockTest.MockTestType == EnumMockTestType.SkillMockTest && mockTestResult.Status == EnumResultStatus.Done)
            {
                await UpdateProcessMockTest(mockTestResult, cancellationToken);
            }
        }
    }
}
