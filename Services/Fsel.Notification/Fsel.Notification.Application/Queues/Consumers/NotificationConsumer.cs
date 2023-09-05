using Fsel.Notification.Application.Commands;
using Fsel.Shared.Models.ShareModels;
using MassTransit;
using MediatR;

namespace Fsel.Notification.Application.Queues.Consumers
{
    public class NotificationConsumer : IConsumer<NotificationQueueModel>
    {
        private readonly IMediator _mediator;

        public NotificationConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<NotificationQueueModel> context)
        {
            var queueConsume = context?.Message;

            if (queueConsume != null)
            {
                CreateNotificationCommand model = new CreateNotificationCommand()
                {
                    Message = queueConsume!.Message,
                    ObjectId = queueConsume.ObjectId,
                    UserId = queueConsume.UserId,
                    Roles = queueConsume.Roles,
                    NotificationTypeId = queueConsume.NotificationTypeId
                };

                await _mediator.Send(model).ConfigureAwait(false);
            }
        }
    }
}
