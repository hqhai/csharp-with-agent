// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Publishers.ExportFiles
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;

    public class ExportFileExcelStudentLearningProcessPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public ExportFileExcelStudentLearningProcessPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(ExportReportStudentLearningProcessQueueModel? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return;
            }

            await _queueProvider.Publish(QueueSettings.LmsQueue.NameQueue.ExportExcelStudentLearningProcess, request, cancellationToken);
        }
    }
}
