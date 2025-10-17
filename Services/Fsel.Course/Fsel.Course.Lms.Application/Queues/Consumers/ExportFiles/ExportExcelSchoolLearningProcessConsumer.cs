// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Consumers.ExportFiles
{
    using Fsel.Core.Base;
    using Fsel.Course.Lms.Application.Queries.Reports;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;

    public class ExportExcelSchoolLearningProcessConsumer : BaseConsumer<ExportReportSchoolLearningProcessQueueModel>
    {
        private readonly IMediator _mediator;

        public ExportExcelSchoolLearningProcessConsumer(IMediator mediator, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(ExportReportSchoolLearningProcessQueueModel? message)
        {
            if (message == null)
            {
                return;
            }

            await _mediator.Send(new ExportReportLearningProcessToSchoolsQuery
            {
                FileName = message.FileName,
                CourseType = message.CourseType,
                DistrictName = message.DistrictName,
                EventCodeStr = message.EventCodeStr,
                CourseLevel = message.CourseLevel,
            }).ConfigureAwait(false);
        }
    }
}
