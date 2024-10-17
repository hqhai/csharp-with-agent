using System.Globalization;
using Fsel.Common.Helpers;
using Fsel.Core.Base;
using Fsel.Notification.Application.Commands;
using Fsel.Notification.Domain.IRepositories;
using Fsel.Notification.Infrastructure.ValueSettings;
using Fsel.Shared.Models.ShareModels;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Fsel.Notification.Application.Queues.Consumers
{
    public class SendNotificationConsumer : BaseConsumer<NotificationSendingQueueModel>
    {
        private readonly IMediator _mediator;
        private readonly INotificationTypeRepository _notificationTypeRepository;
        private readonly AppSetting _appSetting;
        private readonly ILogger<SendNotificationConsumer> _logger;

        public SendNotificationConsumer(IMediator mediator, INotificationTypeRepository notificationTypeRepository,
            AppSetting appSetting,
            AuthContext authContext,
            Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor,
            ILogger<SendNotificationConsumer> logger
            ) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
            _notificationTypeRepository = notificationTypeRepository;
            _appSetting = appSetting;
            _logger = logger;
        }

        public override async Task ConsumeQueue(NotificationSendingQueueModel? message)
        {
            if (message != null)
            {
                var notificationType = await _notificationTypeRepository.Queryable.FirstOrDefaultAsync(x => x.Type == message.Type && x.Content == message.Content);

                string messageNoti = message.ParamsMessage != null ? string.Format(CultureInfo.InvariantCulture, notificationType?.TemplateMessage ?? string.Empty, message.ParamsMessage.ToArray()) : notificationType?.TemplateMessage!;

                string link = message.ParamsLink != null ? string.Format(CultureInfo.InvariantCulture, notificationType?.TemplateLink ?? string.Empty, message.ParamsLink.ToArray()) : notificationType?.TemplateLink!;

                CreateNotificationCommand model = new CreateNotificationCommand()
                {
                    UserIds = message.UserIds,
                    ObjectId = message.ObjectId,
                    NotificationTypeId = notificationType?.Id ?? default,
                    SenderId = message.SenderId,
                    Message = messageNoti,
                    Link = link
                };
                if (message.Content == Shared.Enums.EnumNotificationContent.FselFlowFBEvent)
                {
                    _logger.LogInformation("Consumer Notify FselEvent :" + model.Serialize());
                }
                await _mediator.Send(model).ConfigureAwait(false);
            }
        }
    }
}
