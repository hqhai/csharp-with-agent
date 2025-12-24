// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Publishers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Models.QueryModels.ClassForumAutoDot;
    using Fsel.Shared.Constants;

    public class SubmitClassForumGradingPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public SubmitClassForumGradingPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(ClassForumAIResponseModelV2? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return;
            }
            await _queueProvider.Publish(QueueSettings.LmsQueue.NameQueue.ClassForumAIResponse, request, cancellationToken);
        }
    }
}
