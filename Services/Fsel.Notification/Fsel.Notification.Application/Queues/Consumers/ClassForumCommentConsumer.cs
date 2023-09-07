using Fsel.Notification.Application.Commands;
using Fsel.Notification.Domain.Entities;
using Fsel.Notification.Domain.IRepositories;
using Fsel.Shared.Enums;
using Fsel.Shared.Models.ShareModels;
using MassTransit;
using MediatR;

namespace Fsel.Notification.Application.Queues.Consumers
{
    public class ClassForumCommentConsumer : IConsumer<ClassForumCommentQueueModel>
    {
        private readonly IMediator _mediator;
        private readonly INotificationTypeRepository _notificationTypeRepository;

        public ClassForumCommentConsumer(IMediator mediator, INotificationTypeRepository notificationTypeRepository)
        {
            _mediator = mediator;
            _notificationTypeRepository = notificationTypeRepository;
        }

        public async Task Consume(ConsumeContext<ClassForumCommentQueueModel> context)
        {
            var dataReceipt = context?.Message;

            if (dataReceipt != null)
            {

                var notificationType = _notificationTypeRepository.Queryable.Where(x => x.Type == EnumNotificationPushingType.LinkComment && x.Content == EnumNotificationContent.ClassForum).Select(x => new NotificationMessage
                {
                    Id = x.Id,
                }).FirstOrDefault();

                CreateNotificationCommand model = new CreateNotificationCommand()
                {
                    UserId = dataReceipt.UserId,
                    ObjectId = dataReceipt.ObjectId,
                    NotificationTypeId = notificationType!.Id,
                    Message = dataReceipt.Message
                };
                await _mediator.Send(model).ConfigureAwait(false);

            }
        }
    }
}
