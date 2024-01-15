// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queues.Consumers
{
    using MassTransit;
    using MediatR;
    using Fsel.Course.Domain.Models.CommandModels.AiGradeSetting;
    using Fsel.Course.Application.Commands.AiGradeSettingCmd;
    using Fsel.Course.Domain.Models.CommandModels.Sections;

    public class CreateAiGradeSettingConsumer : IConsumer<AiGradeSettingFeatureModel>
    {
        private readonly IMediator _mediator;

        public CreateAiGradeSettingConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<AiGradeSettingFeatureModel> context)
        {
            if (context == null)
            {
                return;
            }
            var data = context.Message;

            await _mediator.Send(new CreateAiGradeSettingCmd()
            {
                AiGradeSettingModels = data.AiGradeSettingModels
            }).ConfigureAwait(false);
        }
    }
}
