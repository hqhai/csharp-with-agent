// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Lms.Application.Queues.Publishers
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;

    public class GetTimeModulePublisher
    {
        private readonly IQueueProvider _queueProvider;

        public GetTimeModulePublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(GetTimeModuleModel? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return;
            }
            await _queueProvider.Publish(QueueSettings.ExamPracticeQueue.NameQueue.GetTimeExamPractice, request, cancellationToken);
        }
    }
}
