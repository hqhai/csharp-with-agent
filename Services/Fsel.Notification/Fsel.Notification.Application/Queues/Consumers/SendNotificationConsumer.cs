using System.Globalization;
using Fsel.Common.Helpers;
using Fsel.Notification.Application.Commands;
using Fsel.Notification.Domain.IRepositories;
using Fsel.Notification.Infrastructure.ValueSettings;
using Fsel.Shared.Models.ShareModels;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Fsel.Notification.Application.Queues.Consumers
{
    public class SendNotificationConsumer : IConsumer<NotificationQueueModel>
    {
        private readonly IMediator _mediator;
        private readonly INotificationTypeRepository _notificationTypeRepository;
        private readonly ILogger<SendNotificationConsumer> _logger;
        private readonly AppSetting _appSetting;
        public SendNotificationConsumer(IMediator mediator, INotificationTypeRepository notificationTypeRepository, ILogger<SendNotificationConsumer> logger, AppSetting appSetting)
        {
            _mediator = mediator;
            _notificationTypeRepository = notificationTypeRepository;
            _logger = logger;
            _appSetting = appSetting;
        }

        public async Task Consume(ConsumeContext<NotificationQueueModel> context)
        {
            var dataReceipt = context?.Message;

            if (dataReceipt != null)
            {
                var notificationType = await _notificationTypeRepository.Queryable.FirstOrDefaultAsync(x => x.Type == dataReceipt.Type && x.Content == dataReceipt.Content);

                string message = dataReceipt.ParamsMessage != null ? string.Format(CultureInfo.InvariantCulture, notificationType?.TemplateMessage ?? string.Empty, dataReceipt.ParamsMessage.ToArray()) : notificationType?.TemplateMessage!;

                string link = dataReceipt.ParamsLink != null ? string.Format(CultureInfo.InvariantCulture, notificationType?.TemplateLink ?? string.Empty, dataReceipt.ParamsLink.ToArray()) : notificationType?.TemplateLink!;

                _logger.LogInformation($"SendNotificationConsumer: {dataReceipt.UserId}");
                CreateNotificationCommand model = new CreateNotificationCommand()
                {
                    UserId = dataReceipt.UserId ?? default,
                    ObjectId = dataReceipt.ObjectId,
                    Message = message,
                    Roles = dataReceipt.Roles,
                    NotificationTypeId = notificationType?.Id ?? default,
                    SenderId = dataReceipt.SenderId,
                    Link = _appSetting.ConstantUrl?.LmsWebsiteDomain?.CombineUrl(link),
                };
                await _mediator.Send(model).ConfigureAwait(false);
                _logger.LogInformation($"SendNotificationConsumer: Sent {dataReceipt.UserId}");
            }
        }
    }
}
