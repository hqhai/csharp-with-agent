using Fsel.Notification.Application.Commands;
using Fsel.Notification.Domain.IRepositories;
using Fsel.Shared.Constants;
using Fsel.Shared.Enums;
using Fsel.Shared.Models.ShareModels;
using MassTransit;
using MediatR;

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

                if (dataReceipt!.Type == EnumInteractionActionType.Flag)
                {
                    var notificationTypeId = _notificationTypeRepository.Queryable.Where(x => x.Type == EnumNotificationPushingType.Text).Select(x => x.Id).FirstOrDefault();

                    CreateNotificationCommand model = new CreateNotificationCommand()
                    {
                        UserId = dataReceipt.UserId,
                        ObjectId = dataReceipt.ObjectId,
                        NotificationTypeId = notificationTypeId
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
