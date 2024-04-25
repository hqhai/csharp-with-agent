// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Consumers
{
    using Fsel.Course.Lms.Application.Commands.AiCmd;
    using MassTransit;
    using MediatR;
    using Fsel.Course.Domain.Models.QueryModels.ClassForumAutoDot;
    using Fsel.Core.Base;

    public class AiFeedBackResponseConsumer : BaseConsumer<MockTestAnswerResponseModel>
    {
        private readonly IMediator _mediator;

        public AiFeedBackResponseConsumer(IMediator mediator, AuthContext authContext) : base(authContext)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(ConsumeContext<Core.Base.BaseModels.BaseQueueDataModel<MockTestAnswerResponseModel>> context)
        {
            if (context == null)
            {
                return;
            }
            var data = context.Message?.Data;

            if (data != null)
            {
                await _mediator.Send(new SubmitMockTestAnswerAICommand
                {
                    SectionId = data.SectionId,
                    WordContent = data.WordContent,
                    MockTestResultId = data.MockTestResultId,
                    SectionGroupId = data.SectionGroupId
                }).ConfigureAwait(false);
            }
        }
    }
}
