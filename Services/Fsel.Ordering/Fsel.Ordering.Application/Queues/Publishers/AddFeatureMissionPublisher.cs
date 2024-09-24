// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queues.Publishers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;

    public class AddFeatureMissionPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public AddFeatureMissionPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(AddFeatureMissionQueueModel? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return;
            }

            await _queueProvider.Publish(QueueSettings.UserQueue.NameQueue.AddFeatureMission, new AddFeatureMissionQueueModel { FeatureUserReferral = request.FeatureUserReferral, ReceiverId = request.ReceiverId, Token = request.Token }, cancellationToken);
        }
    }
}
