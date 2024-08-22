// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Publishers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;

    public class CreateLuckyTicketPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public CreateLuckyTicketPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(LuckyTicketQueueModel? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return;
            }

            await _queueProvider.Publish(QueueSettings.SystemQueue.NameQueue.CreateLuckyTicket, new LuckyTicketQueueModel { LessonResultId = request.LessonResultId }, cancellationToken);
        }
    }
}
