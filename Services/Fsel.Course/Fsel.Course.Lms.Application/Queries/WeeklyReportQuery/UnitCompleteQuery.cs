// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.WeeklyReportQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.InternalEvents;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class UnitCompleteQuery : IRequest<MethodResult<bool>>
    {
        public Guid StudentId { get; set; }
        public Guid UnitId { get; set; }
    }

    public class UnitCompleteQueryHandler : IRequestHandler<UnitCompleteQuery, MethodResult<bool>>
    {
        private readonly BaseInternalUnitResultEventHandler _baseInternalUnitResult;
        private readonly IUnitRepository _unitRepository;
        private readonly ILessonResultRepository _lessonResultRepository;

        public UnitCompleteQueryHandler(BaseInternalUnitResultEventHandler baseInternalUnitResult, ILessonResultRepository lessonResultRepository, IUnitRepository unitRepository)
        {
            _baseInternalUnitResult = baseInternalUnitResult;
            _lessonResultRepository = lessonResultRepository;
            _unitRepository = unitRepository;
        }

        public async Task<MethodResult<bool>> Handle(UnitCompleteQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var unit = await _unitRepository.GetByIdAsync(request.UnitId);
            if (unit == null)
            {
                methodResult.Result = false;
                return methodResult;
            }

            var lessonResult = await _lessonResultRepository.Queryable.Where(p => p.UnitId == request.UnitId && p.StudentId == request.StudentId).ToListAsync(cancellationToken);

            if (lessonResult.Count == 0 || lessonResult == null)
            {
                methodResult.Result = false;
                return methodResult;
            }

            var courseId = lessonResult.First().CourseId;

            await _baseInternalUnitResult.UpdateUnitResultAsync(lessonResult, unit, courseId, request.StudentId, true, cancellationToken, false);

            methodResult.Result = true;

            return methodResult;
        }
    }
}
