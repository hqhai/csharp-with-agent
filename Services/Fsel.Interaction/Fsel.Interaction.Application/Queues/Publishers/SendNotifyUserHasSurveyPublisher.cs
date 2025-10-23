// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queues.Publishers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;

    public class SendNotifyUserHasSurveyPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public SendNotifyUserHasSurveyPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(SaveUserSurveyAssignmentCommandModel model, CancellationToken cancellationToken)
        {
            if (model == null)
            {
                return;
            }

            await _queueProvider.Publish(QueueSettings.InteractionQueue.NameQueue.SendNotifyUserHasSurvey, new SaveUserSurveyAssignmentCommandModel
            {
                CourseLevel = model.CourseLevel,
                CourseType = model.CourseType,
                ProgressRequirement = model.ProgressRequirement
            }, cancellationToken);
        }
    }
}
