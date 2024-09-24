// Copyright (c) Atlantic. All rights reserved.
using Fsel.Core.Base.Interfaces;
using Fsel.Shared.Constants;
using Fsel.Shared.Models.ShareModels;

namespace Fsel.Course.Lms.Application.Queues.Publishers
{
    public class QuestBoardPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public QuestBoardPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(QuestBoardQueueModel model, CancellationToken cancellationToken)
        {
            if (model == null)
            {
                return;
            }

            await _queueProvider.Publish(QueueSettings.SystemQueue.NameQueue.QuestBoard, new QuestBoardQueueModel
            {
                StudentID = model.StudentID,
                Type = model.Type,
                Category = model.Category,
                Value = model.Value,
            }, cancellationToken);
        }
    }
}
