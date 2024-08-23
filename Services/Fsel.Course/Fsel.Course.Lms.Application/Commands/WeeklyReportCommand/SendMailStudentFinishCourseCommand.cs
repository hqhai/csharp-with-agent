// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.WeeklyReportCommand
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.InternalEvents;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class SendMailStudentFinishCourseCommand : IRequest<MethodResult<bool>>
    {
        public Guid StudentId { get; set; }
        public Guid CourseId { get; set; }
    }

    public class SendMailStudentFinishCourseCommandHandler : IRequestHandler<SendMailStudentFinishCourseCommand, MethodResult<bool>>
    {
        private readonly BaseInternalEventHandler _baseInternalEventHandler;
        private readonly ICourseResultRepository _courseResultRepository;

        public SendMailStudentFinishCourseCommandHandler(BaseInternalEventHandler baseInternalEventHandler, ICourseResultRepository courseResultRepository)
        {
            _baseInternalEventHandler = baseInternalEventHandler;
            _courseResultRepository = courseResultRepository;
        }

        public async Task<MethodResult<bool>> Handle(SendMailStudentFinishCourseCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var courseResult = await _courseResultRepository.Queryable.FirstOrDefaultAsync(p => p.StudentId == request.StudentId && p.CourseId == request.CourseId, cancellationToken);
            if (courseResult == null || courseResult.Status != EnumResultStatus.Done)
            {
                methodResult.AddErrorBadRequest("CourseResult null or not Done");
                return methodResult;
            }
            await _baseInternalEventHandler.SendStudentCompleteCourse(request.StudentId, request.CourseId, courseResult, cancellationToken);
            methodResult.Result = true;
            return methodResult;
        }
    }
}
