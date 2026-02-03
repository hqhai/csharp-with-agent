// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Publishers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Lms.Application.Commands.OtherCmd;
    using Fsel.Shared.Constants;

    public class SendMailFinishPTPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public SendMailFinishPTPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(SendMailFinishPTModel? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return;
            }
            await _queueProvider.Publish(QueueSettings.LmsQueue.NameQueue.SendMailFinishPT, request, cancellationToken);
        }
    }
}
