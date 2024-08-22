using System.Globalization;
using Fsel.Core.Base;
using Fsel.Core.Base.BaseModels;
using Fsel.Notification.Application.Commands;
using Fsel.Notification.Domain.IRepositories;
using Fsel.Shared.Enums;
using Fsel.Shared.Models.ShareModels;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Notification.Application.Queues.Consumers
{
    public class InterationActionConsumer : BaseConsumer<InterationActionQueueModel>
    {
        private readonly IMediator _mediator;
        private readonly INotificationTypeRepository _notificationTypeRepository;

        public InterationActionConsumer(IMediator mediator, INotificationTypeRepository notificationTypeRepository, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
            _notificationTypeRepository = notificationTypeRepository;
        }

        public override async Task ConsumeQueue(InterationActionQueueModel? message)
        {
            if (message != null)
            {

                var notificationType = await _notificationTypeRepository.Queryable.FirstOrDefaultAsync(x => x.Type == message.Type && x.Content == message.Content);

                string messageNoti = message.ParamsMessage != null ? string.Format(CultureInfo.InvariantCulture, notificationType?.TemplateMessage ?? string.Empty, message.ParamsMessage.ToArray()) : "";

                string link = message.ParamsLink != null ? string.Format(CultureInfo.InvariantCulture, notificationType?.TemplateLink ?? string.Empty, message.ParamsLink.ToArray()) : "";


                if (message!.InterationType == EnumInteractionActionType.Flag)
                {
                    CreateNotificationCommand model = new CreateNotificationCommand()
                    {
                        UserIds = message.UserIds ?? default,
                        ObjectId = message.ObjectId,
                        Message = messageNoti,
                        Link = link,
                        Roles = message.Roles,
                        NotificationTypeId = notificationType?.Id ?? default,
                        SenderId = message.SenderId ?? default,
                    };
                    await _mediator.Send(model).ConfigureAwait(false);
                }
                else if (message!.InterationType == EnumInteractionActionType.Like)
                {
                    UpdateNotificationCommand model = new UpdateNotificationCommand()
                    {
                        UserId = message.UserIds!.Single(),
                        ObjectId = message.ObjectId,
                        Message = messageNoti,
                        Link = link,
                        NotificationTypeId = notificationType?.Id ?? default,
                        SenderId = message.SenderId ?? default
                    };
                    await _mediator.Send(model).ConfigureAwait(false);
                }
                else
                {
                    CreateNotificationRemindCommand cmd = new CreateNotificationRemindCommand()
                    {
                        ObjectId = message.ObjectId,
                        UserIds = message.UserIds,
                        Status = EnumNotificationRemindStatus.Off
                    };

                    await _mediator.Send(cmd).ConfigureAwait(false);
                }
            }
        }
    }
}
