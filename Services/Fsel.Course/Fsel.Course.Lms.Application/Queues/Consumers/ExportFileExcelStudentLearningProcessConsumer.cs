// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Consumers
{
    using Fsel.Core.Base;
    using Fsel.Course.Lms.Application.Queries.Reports;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;

    public class ExportFileExcelStudentLearningProcessConsumer : BaseConsumer<ExportReportStudentLearningProcessQueueModel>
    {
        private readonly IMediator _mediator;

        public ExportFileExcelStudentLearningProcessConsumer(IMediator mediator, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(ExportReportStudentLearningProcessQueueModel? message)
        {
            if (message == null)
            {
                return;
            }

            await _mediator.Send(new ExportReportLearningProcessStudentQuery
            {
                FileName = message.FileName,
                CourseType = message.CourseType,
                DistrictName = message.DistrictName,
                EventCodeStr = message.EventCodeStr,
                StudentId = message.StudentId,
                UserNameStr = message.UserNameStr,
            }).ConfigureAwait(false);
        }
    }
}
