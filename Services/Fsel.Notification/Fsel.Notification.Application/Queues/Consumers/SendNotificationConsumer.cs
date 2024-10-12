using System.Globalization;
using Fsel.Common.Helpers;
using Fsel.Core.Base;
using Fsel.Notification.Application.Commands;
using Fsel.Notification.Domain.IRepositories;
using Fsel.Notification.Domain.Model.CommandModels.Notification;
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

        private readonly ILogger<SendNotificationConsumer> _logger;

        public SendNotificationConsumer(IMediator mediator, INotificationTypeRepository notificationTypeRepository, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor,
            ILogger<SendNotificationConsumer> logger) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
            _notificationTypeRepository = notificationTypeRepository;
            _logger = logger;
        }

        public override async Task ConsumeQueue(NotificationSendingQueueModel? message)
        {
            if (message != null)
            {
                var notificationType = await _notificationTypeRepository.Queryable.Include(x => x.Translations).FirstOrDefaultAsync(x => x.Type == message.Type && x.Content == message.Content);

                string messageNoti = message.ParamsMessage != null ? string.Format(CultureInfo.InvariantCulture, notificationType?.TemplateMessage ?? string.Empty, message.ParamsMessage.ToArray()) : notificationType?.TemplateMessage!;

                List<NotificationMessageTranslationModel> notificationTranslations = new List<NotificationMessageTranslationModel>();

                if (notificationType != null)
                {
                    foreach (var item in notificationType.Translations)
                    {
                        string translationMessage = message.ParamsMessage != null ? string.Format(CultureInfo.InvariantCulture, item?.TemplateMessage ?? string.Empty, message.ParamsMessage.ToArray()) : item?.TemplateMessage!;

                        NotificationMessageTranslationModel translation = new NotificationMessageTranslationModel
                        {
                            Language = item?.Language ?? "vi-VN",
                            Message = translationMessage
                        };

                        notificationTranslations.Add(translation);
                    }
                }

                string link = message.ParamsLink != null ? string.Format(CultureInfo.InvariantCulture, notificationType?.TemplateLink ?? string.Empty, message.ParamsLink.ToArray()) : notificationType?.TemplateLink!;

                CreateNotificationCommand model = new CreateNotificationCommand()
                {
                    UserIds = message.UserIds,
                    ObjectId = message.ObjectId,
                    NotificationTypeId = notificationType?.Id ?? default,
                    SenderId = message.SenderId,
                    Translations = notificationTranslations,
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
