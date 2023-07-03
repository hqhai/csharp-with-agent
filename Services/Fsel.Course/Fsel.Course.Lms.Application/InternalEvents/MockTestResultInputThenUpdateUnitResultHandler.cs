// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Core.Applications.InternalEvents;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class MockTestResultInputThenUpdateUnitResultHandler : BaseInternalEventHandler,
        INotificationHandler<EntityChangedEvent<MockTestResult>>
    {
        private readonly IUnitRepository _unitRepository;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly IMockTestRepository _mockTestRepository;

        public MockTestResultInputThenUpdateUnitResultHandler(IUnitRepository unitRepository
            , IUnitResultRepository unitResultRepository
            , ILessonResultRepository lessonResultRepository
            , IVideoResultRepository videoResultRepository
            , IClassForumResultRepository classForumResultRepository
            , IHomeWorkResultRepository homeWorkResultRepository
            , IMockTestRepository mockTestRepository
            ) : base(videoResultRepository, classForumResultRepository, unitResultRepository, lessonResultRepository, homeWorkResultRepository)
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
            var unit = await _unitRepository.Queryable.Include(x => x.UnitLessons)
                                                   .Include(x => x.LessonResults.Where(x => x.Status == EnumResultStatus.Done && x.StudentId == mockTestResult.StudentId && x.CourseId == mockTestResult.CourseId))
                                                   .FirstOrDefaultAsync(x => x.Id == mockTestResult.UnitId, cancellationToken);
            if (unit != null && mockTest != null && mockTest.MockTestType == EnumMockTestType.SkillMockTest && mockTestResult.Status == EnumResultStatus.Done)
            {
                var unitResult = await _unitResultRepository.Queryable.FirstOrDefaultAsync(x => x.UnitId == mockTestResult.UnitId && x.StudentId == mockTestResult.StudentId && x.CourseId == mockTestResult.CourseId, cancellationToken);
                if (unitResult != null && unit.LessonResults.Count == unit.UnitLessons.Count)
                {
                    await UpdateUnit(unit.LessonResults.ToList(), unitResult, cancellationToken);
                }
            }
        }
    }
}
