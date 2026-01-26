// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.WeeklyReportQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Infrastructure.Repositories;
    using Fsel.Course.Lms.Application.InternalEvents;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SendStudentCompleteUnitCommand : IRequest<MethodResult<bool>>
    {
        public Guid StudentId { get; set; }
        public Guid UnitId { get; set; }
    }

    public class SendStudentCompleteUnitCommandHandler : IRequestHandler<SendStudentCompleteUnitCommand, MethodResult<bool>>
    {
        private readonly BaseInternalUnitResultEventHandler _baseInternalUnitResult;
        private readonly IUnitRepository _unitRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly ICourseResultRepository _courseResultRepository;

        public SendStudentCompleteUnitCommandHandler(BaseInternalUnitResultEventHandler baseInternalUnitResult, ILessonResultRepository lessonResultRepository, IUnitRepository unitRepository, ICourseResultRepository courseResultRepository)
        {
            _baseInternalUnitResult = baseInternalUnitResult;
            _lessonResultRepository = lessonResultRepository;
            _unitRepository = unitRepository;
            _courseResultRepository = courseResultRepository;
        }

        public async Task<MethodResult<bool>> Handle(SendStudentCompleteUnitCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var courseResult = await _courseResultRepository.Queryable.FirstOrDefaultAsync(x => x.StudentId == request.StudentId && x.WorkingStatus == Shared.Enums.EnumWorkingStatus.Active, cancellationToken);
            if (courseResult == null)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            var unit = await _unitRepository.GetByIdAsync(request.UnitId);
            if (unit == null)
            {
                methodResult.Result = false;
                return methodResult;
            }

            var lessonResult = await _lessonResultRepository.Queryable.Where(p => p.CourseResultId == courseResult.Id && p.UnitId == request.UnitId && p.StudentId == request.StudentId).ToListAsync(cancellationToken);
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
