// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Publishers
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;

    public class ExportFileExcelSchoolLearningProcessPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public ExportFileExcelSchoolLearningProcessPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(ExportReportSchoolLearningProcessQueueModel? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return;
            }

            await _queueProvider.Publish(QueueSettings.LmsQueue.NameQueue.ExportExcelSchoolLearningProcess, request, cancellationToken);
        }
    }
}
