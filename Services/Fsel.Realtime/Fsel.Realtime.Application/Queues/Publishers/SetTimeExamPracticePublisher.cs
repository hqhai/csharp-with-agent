// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Realtime.Application.Queues.Publishers
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;

    public class SetTimeExamPracticePublisher
    {
        private readonly IQueueProvider _queueProvider;

        public SetTimeExamPracticePublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(SetTimeModuleModel? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return;
            }
            await _queueProvider.Publish(QueueSettings.ExamPracticeQueue.NameQueue.SetTimeExamPractice, request, cancellationToken);
        }
    }
}
