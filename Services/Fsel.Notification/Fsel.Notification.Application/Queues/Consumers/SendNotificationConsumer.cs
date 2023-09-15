using System.Globalization;
using Fsel.Notification.Application.Commands;
using Fsel.Notification.Domain.IRepositories;
using Fsel.Shared.Models.ShareModels;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Notification.Application.Queues.Consumers
{
    public class SendNotificationConsumer : IConsumer<NotificationQueueModel>
    {
        private readonly IMediator _mediator;
        private readonly INotificationTypeRepository _notificationTypeRepository;

        public SendNotificationConsumer(IMediator mediator, INotificationTypeRepository notificationTypeRepository)
        {
            _mediator = mediator;
            _notificationTypeRepository = notificationTypeRepository;
        }

        public async Task Consume(ConsumeContext<NotificationQueueModel> context)
        {
            var dataReceipt = context?.Message;

            if (dataReceipt != null)
            {
                var notificationType = await _notificationTypeRepository.Queryable.FirstOrDefaultAsync(x => x.Type == dataReceipt.Type && x.Content == dataReceipt.Content);

                string message = dataReceipt.ParamsMessage != null ? string.Format(CultureInfo.InvariantCulture, notificationType?.TemplateMessage ?? string.Empty, dataReceipt.ParamsMessage.ToArray()) : notificationType?.TemplateMessage!;

                string link = dataReceipt.ParamsLink != null ? string.Format(CultureInfo.InvariantCulture, notificationType?.TemplateLink ?? string.Empty, dataReceipt.ParamsLink.ToArray()) : notificationType?.TemplateLink!;

                CreateNotificationCommand model = new CreateNotificationCommand()
                {
                    UserId = dataReceipt.UserId ?? default,
                    ObjectId = dataReceipt.ObjectId,
                    Message = message,
                    Link = link,
                    Roles = dataReceipt.Roles,
                    NotificationTypeId = notificationType?.Id ?? default,
                    SenderId = dataReceipt.SenderId,
                };
                await _mediator.Send(model).ConfigureAwait(false);
            }
        }
    }
}
