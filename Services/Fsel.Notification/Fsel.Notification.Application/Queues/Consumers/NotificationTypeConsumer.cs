using Fsel.Notification.Application.Commands;
using Fsel.Notification.Domain.IRepositories;
using Fsel.Shared.Enums;
using Fsel.Shared.Models.ShareModels;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Notification.Application.Queues.Consumers
{
    public class NotificationTypeConsumer : IConsumer<NotificationAllQueueModel>
    {
        private readonly IMediator _mediator;
        private readonly INotificationTypeRepository _notificationTypeRepository;

        public NotificationTypeConsumer(IMediator mediator, INotificationTypeRepository notificationTypeRepository)
        {
            _mediator = mediator;
            _notificationTypeRepository = notificationTypeRepository;
        }

        public async Task Consume(ConsumeContext<NotificationAllQueueModel> context)
        {
            var dataReceipt = context?.Message;

            if (dataReceipt != null)
            {
                var notificationType = await _notificationTypeRepository.Queryable.FirstOrDefaultAsync(x => x.Type == EnumNotificationPushingType.Text);
                CreateNotificationCommand model = new CreateNotificationCommand()
                {
                    Title = dataReceipt.Title,
                    UserId = dataReceipt.UserId ?? default,
                    ObjectId = dataReceipt.ObjectId,
                    Message = dataReceipt.Message ?? notificationType?.Template,
                    Roles = dataReceipt.Roles,
                    NotificationTypeId = notificationType?.Id ?? default
                };
                await _mediator.Send(model).ConfigureAwait(false);
            }
        }
    }
}
