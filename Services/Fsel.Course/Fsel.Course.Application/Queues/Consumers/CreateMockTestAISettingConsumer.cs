// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queues.Consumers
{
    using MassTransit;
    using MediatR;
    using Fsel.Course.Domain.Models.CommandModels.AiGradeSetting;
    using Fsel.Course.Application.Commands.AiGradeSettingCmd;

    public class CreateMockTestAISettingConsumer : IConsumer<MockTestAiSettingModel>
    {
        private readonly IMediator _mediator;

        public CreateMockTestAISettingConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<MockTestAiSettingModel> context)
        {
            if (context == null)
            {
                return;
            }
            var data = context.Message;

            await _mediator.Send(new CreateMockTestAISettingCmd()
            {
                MockTestAiSettingModels = data.MockTestAiSettingModels
            }).ConfigureAwait(false);
        }
    }
}
