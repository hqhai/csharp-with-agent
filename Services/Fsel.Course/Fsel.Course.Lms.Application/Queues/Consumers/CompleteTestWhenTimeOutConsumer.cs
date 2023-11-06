// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Consumers
{
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Lms.Application.Commands.ClassForumResultCmd;
    using Fsel.Shared.Models.ShareModels;
    using MassTransit;
    using MediatR;

    public class CompleteTestWhenTimeOutConsumer : IConsumer<CompleteTestWhenTimeOutModel>
    {
        private readonly IMediator _mediator;

        public CompleteTestWhenTimeOutConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<CompleteTestWhenTimeOutModel> context)
        {
            if (context == null)
            {
                return;
            }
            var messeger = context.Message;
            switch (messeger.ObjectResultType)
            {
                case nameof(MockTest):
                    await _mediator.Send(new UpdateOcCheckInClassForumResultCommand()).ConfigureAwait(false);
                    break;

                case nameof(PlacementTest):
                    break;

                case nameof(FinalTest):
                    break;

                case nameof(EnumTimeCodeType.SkillTest):
                    break;

                case nameof(EnumTimeCodeType.UnitTest):
                    break;
            }
        }
    }
}
