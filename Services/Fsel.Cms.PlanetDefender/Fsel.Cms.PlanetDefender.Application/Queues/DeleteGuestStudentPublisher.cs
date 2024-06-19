// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Queues
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Constants;

    public class DeleteGuestStudentPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public DeleteGuestStudentPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(Guid id, CancellationToken cancellationToken)
        {
            await _queueProvider.Publish(QueueSettings.PlantDefenderQueue.NameQueue.DeleteGuestStudent, new BaseQueueModel
            {
                QueueId = id.ToString(),
            }, cancellationToken);
        }
    }
}
