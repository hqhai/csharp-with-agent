// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Queues
{
    using System.Threading.Tasks;
    using Fsel.Cms.PlanetDefender.Domain.Entities;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;

    public class DeleteGuestStudentPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public DeleteGuestStudentPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(StudentGameInfo models, CancellationToken cancellationToken)
        {
            if (models == null)
            {
                return;
            }
            await _queueProvider.Publish(QueueSettings.PlantDefenderQueue.NameQueue.DeleteGuestStudent, new DeleteGuestStudentQueueModel
            {
                Id = models.Id,
            }, cancellationToken);
        }
    }
}
