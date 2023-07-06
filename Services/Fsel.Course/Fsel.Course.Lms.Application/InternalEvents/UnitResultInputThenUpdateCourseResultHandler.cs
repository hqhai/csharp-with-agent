// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents
{
    using Fsel.Core.Applications.InternalEvents;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class UnitResultInputThenUpdateCourseResultHandler : BaseInternalEventHandler,
        INotificationHandler<EntityChangedEvent<UnitResult>>
    {
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly ICourseRepository _courseRepository;

        public UnitResultInputThenUpdateCourseResultHandler(IUnitResultRepository unitResultRepository
            , ILessonResultRepository lessonResultRepository
            , IVideoResultRepository videoResultRepository
            , IClassForumResultRepository classForumResultRepository
            , IHomeWorkResultRepository homeWorkResultRepository
            , ICourseRepository courseRepository
            ) : base(videoResultRepository, classForumResultRepository, unitResultRepository, lessonResultRepository, homeWorkResultRepository)
        {
            _unitResultRepository = unitResultRepository;
            _courseRepository = courseRepository;
        }

        public async Task Handle(EntityChangedEvent<UnitResult> notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            var unitResult = notification.Data;
            var course = await _courseRepository.Queryable.Include(x => x.CourseUnitMockTests)
                                                  .Include(x => x.UnitResults)
                                                  .Include(x => x.MockTestResults)
                                                  .Include(x => x.FinalTestResults)
                                                  .FirstOrDefaultAsync(x => x.Id == unitResult.CourseId, cancellationToken);
            if (course != null && unitResult.Status == EnumResultStatus.Done)
            {
                var isCheckDone = false;
                var isCheckUnitResults = course.UnitResults.Where(x => x.StudentId == unitResult.StudentId).All(x => x.Status == EnumResultStatus.Done);
                switch (course.CourseLevel.GetEnumCourseType())
                {
                    case EnumCourseType.Ielts:
                        isCheckDone = isCheckUnitResults && course.MockTestResults.Where(x => x.StudentId == unitResult.StudentId).All(x => x.Status == EnumResultStatus.Done);
                        break;

                    case EnumCourseType.Academic:
                        isCheckDone = isCheckUnitResults && course.FinalTestResults.Where(x => x.StudentId == unitResult.StudentId).All(x => x.Status == EnumResultStatus.Done);
                        break;
                }
                var displayOrder = course.CourseUnitMockTests.FirstOrDefault(x => x.UnitId == unitResult.UnitId)?.DisplayOrder;
                var courseUnitMockTest = course.CourseUnitMockTests.FirstOrDefault(x => x.DisplayOrder == displayOrder + 1);
                if (!isCheckDone && courseUnitMockTest != null)
                {
                    switch (false)
                    {
                        case var value when value == (courseUnitMockTest.UnitId == null):
                            var unitResultNext = await _unitResultRepository.Queryable.FirstOrDefaultAsync(x => x.StudentId == unitResult.StudentId && x.UnitId == courseUnitMockTest.UnitId, cancellationToken);
                            if (unitResultNext != null)
                            {
                                unitResultNext.Status = EnumResultStatus.Process;
                            }
                            break;

                        case var value when value == (courseUnitMockTest.FinalTestId == null):

                            break;

                        case var value when value == (courseUnitMockTest.MockTestId == null):

                            break;

                        default:
                            break;
                    }
                }
            }
        }
    }
}
