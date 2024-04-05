// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.WeeklyReportCommand
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.InternalEvents;
    using Fsel.Course.Lms.Application.Queries.WeeklyReportQuery;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class SendStudentCompleteMidCourseCommand : IRequest<MethodResult<bool>>
    {
        public IList<Guid>? StudentIds { get; set; }
    }

    public class SendStudentCompleteMidCourseCommandHandler : IRequestHandler<SendStudentCompleteMidCourseCommand, MethodResult<bool>>
    {
        private readonly BaseInternalUnitResultEventHandler _baseInternalUnitResultEvent;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly ICourseRepository _courseRepository;

        public SendStudentCompleteMidCourseCommandHandler(BaseInternalUnitResultEventHandler baseInternalUnitResultEvent, IUnitResultRepository unitResultRepository, ICourseRepository courseRepository)
        {
            _baseInternalUnitResultEvent = baseInternalUnitResultEvent;
            _unitResultRepository = unitResultRepository;
            _courseRepository = courseRepository;
        }

        public async Task<MethodResult<bool>> Handle(SendStudentCompleteMidCourseCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            if (request.StudentIds == null || request.StudentIds.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            foreach (var item in request.StudentIds)
            {
                var unitResult = await _unitResultRepository.Queryable.Where(p => p.StudentId == item).OrderByDescending(p => p.CreatedDate).FirstOrDefaultAsync(cancellationToken);
                if (unitResult == null)
                {
                    continue;
                }
                var course = await _courseRepository.Queryable.Include(p => p.CourseUnitMockTests).FirstOrDefaultAsync(p => p.Id == unitResult.CourseId, cancellationToken);
                if (course == null)
                {
                    continue;
                }
                await _baseInternalUnitResultEvent.SendMailMidCourseReport(item, course, cancellationToken);
            }
            methodResult.Result = true;
            return methodResult;
        }
    }
}
