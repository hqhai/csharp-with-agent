// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Consumers
{
    using Fsel.Course.Lms.Application.Commands.AiCmd;
    using MassTransit;
    using MediatR;
    using Fsel.Course.Domain.Models.QueryModels.ClassForumAutoDot;

    public class AiFeedBackResponseConsumer : IConsumer<MockTestAnswerResponseModel>
    {
        private readonly IMediator _mediator;

        public AiFeedBackResponseConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<MockTestAnswerResponseModel> context)
        {
            if (context == null)
            {
                return;
            }
            var data = context.Message;

            await _mediator.Send(new SubmitMockTestAnswerAICommand
            {
                SectionId = data.SectionId,
                WordContent = data.WordContent,
                MockTestResultId = data.MockTestResultId,
                SectionGroupId = data.SectionGroupId,
            }).ConfigureAwait(false);
        }
    }
}
