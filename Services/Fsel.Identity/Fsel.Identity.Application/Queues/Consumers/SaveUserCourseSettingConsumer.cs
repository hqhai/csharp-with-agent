// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queues.Consumers
{
    using System.Threading.Tasks;
    using Fsel.Identity.Application.Commands.UserCourseSettingCmd;
    using Fsel.Shared.Models.ShareModels;
    using MassTransit;
    using MediatR;

    public class SaveUserCourseSettingConsumer : IConsumer<SaveUserCourseSettingQueueModel>
    {
        private readonly IMediator _mediator;

        public SaveUserCourseSettingConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<SaveUserCourseSettingQueueModel> context)
        {
            var message = context?.Message;
            if (message == null)
            {
                return;
            }

            await _mediator.Send(new SaveUserCourseSettingCommand
            {
                UserId = message.UserId,
                Type = message.Type,
                IsCreate = message.IsCreate,
                CourseLevel = message.CourseLevel,
                IsDeduction = message.IsDeduction
            }).ConfigureAwait(false);
        }
    }
}
