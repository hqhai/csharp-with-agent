// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Notification.Application.Queues.Consumers
{
    using System.Threading.Tasks;
    using Fsel.Notification.Application.Commands;
    using Fsel.Notification.Domain.IRepositories;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MassTransit;
    using MediatR;

    public class CreateClassForumResultConsumer : IConsumer<CreateClassForumResultQueueModel>
    {
        private readonly IMediator _mediator;
        private readonly INotificationTypeRepository _notificationTypeRepository;

        public CreateClassForumResultConsumer(IMediator mediator, INotificationTypeRepository notificationTypeRepository)
        {
            _mediator = mediator;
            _notificationTypeRepository = notificationTypeRepository;
        }

        public async Task Consume(ConsumeContext<CreateClassForumResultQueueModel> context)
        {
            var dataReceipt = context?.Message;

            if (dataReceipt != null)
            {
                var notificationTypeId = _notificationTypeRepository.Queryable.Where(x => x.Type == EnumNotificationPushingType.Text).Select(x => x.Id).FirstOrDefault();
                IList<EnumRole> roles = new List<EnumRole>();
                roles.Add(dataReceipt.Role);
                CreateNotificationCommand model = new CreateNotificationCommand()
                {
                    ObjectId = dataReceipt.ObjectId,
                    Message = NotificationTemplateSetting.CreateClassForumResultTemplate,
                    Roles = roles,
                    NotificationTypeId = notificationTypeId
                };
                await _mediator.Send(model).ConfigureAwait(false);
            }
        }
    }
}
