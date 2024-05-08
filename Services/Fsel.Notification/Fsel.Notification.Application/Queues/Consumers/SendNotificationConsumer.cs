using System.Globalization;
using Fsel.Common.Helpers;
using Fsel.Core.Base.BaseModels;
using Fsel.Notification.Application.Commands;
using Fsel.Notification.Domain.IRepositories;
using Fsel.Notification.Infrastructure.ValueSettings;
using Fsel.Shared.Enums;
using Fsel.Shared.Models.ShareModels;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Notification.Application.Queues.Consumers
{
    public class SendNotificationConsumer : Core.Base.Interfaces.IBaseConsumer<NotificationSendingQueueModel>
    {
        private readonly IMediator _mediator;
        private readonly INotificationTypeRepository _notificationTypeRepository;
        private readonly AppSetting _appSetting;

        public SendNotificationConsumer(IMediator mediator, INotificationTypeRepository notificationTypeRepository, AppSetting appSetting)
        {
            _mediator = mediator;
            _notificationTypeRepository = notificationTypeRepository;
            _appSetting = appSetting;
        }

        public async Task Consume(ConsumeContext<BaseQueueDataModel<NotificationSendingQueueModel>> context)
        {
            var dataReceipt = context?.Message?.Data;

            if (dataReceipt != null)
            {
                var notificationType = await _notificationTypeRepository.Queryable.FirstOrDefaultAsync(x => x.Type == dataReceipt.Type && x.Content == dataReceipt.Content);

                string message = dataReceipt.ParamsMessage != null ? string.Format(CultureInfo.InvariantCulture, notificationType?.TemplateMessage ?? string.Empty, dataReceipt.ParamsMessage.ToArray()) : notificationType?.TemplateMessage!;

                string link = dataReceipt.ParamsLink != null ? string.Format(CultureInfo.InvariantCulture, notificationType?.TemplateLink ?? string.Empty, dataReceipt.ParamsLink.ToArray()) : notificationType?.TemplateLink!;

                CreateNotificationCommand model = new CreateNotificationCommand()
                {
                    UserIds = dataReceipt.UserIds,
                    ObjectId = dataReceipt.ObjectId,
                    NotificationTypeId = notificationType?.Id ?? default,
                    SenderId = dataReceipt.SenderId,
                    Message = message,
                    Link = link
                };
                await _mediator.Send(model).ConfigureAwait(false);
            }
        }
    }
}
