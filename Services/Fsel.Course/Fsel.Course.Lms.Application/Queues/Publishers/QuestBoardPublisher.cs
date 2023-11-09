// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Publishers
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;

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
                StudentId = model.StudentId,
                AchievedPoint = model.AchievedPoint,
                Categories = model.Categories,
                ObjectId = model.ObjectId,
                CourseId = model.CourseId,
            }, cancellationToken);
        }
    }
}
