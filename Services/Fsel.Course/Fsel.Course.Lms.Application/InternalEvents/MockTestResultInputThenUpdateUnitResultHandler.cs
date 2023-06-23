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

    public class MockTestResultInputThenUpdateUnitResultHandler :
        INotificationHandler<EntityChangedEvent<MockTestResult>>
    {
        private readonly IUnitRepository _unitRepository;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly IMockTestRepository _mockTestRepository;

        public MockTestResultInputThenUpdateUnitResultHandler(IUnitRepository unitRepository
            , IUnitResultRepository unitResultRepository
            , IMockTestResultRepository mockTestResultRepository
            , IMockTestRepository mockTestRepository)
        {
            _unitRepository = unitRepository;
            _unitResultRepository = unitResultRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _mockTestRepository = mockTestRepository;
        }

        public async Task Handle(EntityChangedEvent<MockTestResult> notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            var mockTest = await _mockTestRepository.GetByIdAsync(notification.Data.MockTestId);
            if (mockTest != null && mockTest.MockTestType == EnumMockTestType.SkillMockTest)
            {
                var unit = await _unitRepository.Queryable.Include(x => x.LessonResults.Where(x => x.Status == EnumResultStatus.Done && x.StudentId == notification.Data.StudentId && x.CourseId == notification.Data.CourseId))
                                                    .FirstOrDefaultAsync(x => x.Id == notification.Data.UnitId, cancellationToken);

                var mockTestResult = await _mockTestResultRepository.Queryable.FirstOrDefaultAsync(x => x.Status == EnumResultStatus.Done && x.StudentId == notification.Data.StudentId && x.CourseId == notification.Data.CourseId && x.UnitId == notification.Data.UnitId, cancellationToken);
                if (unit != null)
                {
                    var unitResult = await _unitResultRepository.Queryable.FirstOrDefaultAsync(x => x.UnitId == notification.Data.UnitId && x.StudentId == notification.Data.StudentId && x.CourseId == notification.Data.CourseId, cancellationToken);
                    if (unitResult != null)
                    {
                        unitResult.Status = EnumResultStatus.Done;
                        unitResult.Percent += notification.Data.Percent * 18 / 100;
                        _unitResultRepository.Update(unitResult);
                        await _unitResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                    }
                }
            }
        }
    }
}
