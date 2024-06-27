// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queues.Publishers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;

    public class SaveUserCourseSettingPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public SaveUserCourseSettingPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(SaveUserCourseSettingQueueModel? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return;
            }
            await _queueProvider.Publish(QueueSettings.TrainingQueue.NameQueue.SaveUserCourseSetting, request, cancellationToken);
        }
    }
}
