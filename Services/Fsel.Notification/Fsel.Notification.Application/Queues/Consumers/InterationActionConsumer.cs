using System.Globalization;
using Fsel.Notification.Application.Commands;
using Fsel.Notification.Domain.IRepositories;
using Fsel.Shared.Enums;
using Fsel.Shared.Models.ShareModels;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Notification.Application.Queues.Consumers
{
    public class InterationActionConsumer : IConsumer<InterationActionQueueModel>
    {
        private readonly IMediator _mediator;
        private readonly INotificationTypeRepository _notificationTypeRepository;

        public InterationActionConsumer(IMediator mediator, INotificationTypeRepository notificationTypeRepository)
        {
            _mediator = mediator;
            _notificationTypeRepository = notificationTypeRepository;
        }

        public async Task Consume(ConsumeContext<InterationActionQueueModel> context)
        {
            var dataReceipt = context?.Message;

            if (dataReceipt != null)
            {
                if (dataReceipt!.InterationType == EnumInteractionActionType.Flag)
                {
                    var notificationType = await _notificationTypeRepository.Queryable.FirstOrDefaultAsync(x => x.Type == dataReceipt.Type && x.Content == dataReceipt.Content);

                    string message = dataReceipt.ParamsMessage != null ? string.Format(CultureInfo.InvariantCulture, notificationType?.TemplateMessage ?? string.Empty, dataReceipt.ParamsMessage.ToArray()) : "";

                    string link = dataReceipt.ParamsLink != null ? string.Format(CultureInfo.InvariantCulture, notificationType?.TemplateLink ?? string.Empty, dataReceipt.ParamsLink.ToArray()) : "";

                    CreateNotificationCommand model = new CreateNotificationCommand()
                    {
                        UserId = dataReceipt.UserId ?? default,
                        ObjectId = dataReceipt.ObjectId,
                        Message = message,
                        Link = link,
                        Roles = dataReceipt.Roles,
                        NotificationTypeId = notificationType?.Id ?? default
                    };
                    await _mediator.Send(model).ConfigureAwait(false);
                }
                else
                {
                    CreateNotificationRemindCommand cmd = new CreateNotificationRemindCommand()
                    {
                        ObjectId = dataReceipt.ObjectId,
                        UserId = dataReceipt.UserId,
                        Status = EnumNotificationRemindStatus.Off
                    };

                    await _mediator.Send(cmd).ConfigureAwait(false);
                }
            }
        }
    }
}
