// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Consumers
{
    using Fsel.Course.Lms.Application.Commands.AiCmd;
    using MassTransit;
    using MediatR;
    using Fsel.Course.Domain.Models.QueryModels.ClassForumAutoDot;

    public class RealTimeAIResponseConsumer : IConsumer<ClassForumAIResponseModel>
    {
        private readonly IMediator _mediator;

        public RealTimeAIResponseConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<ClassForumAIResponseModel> context)
        {
            if (context == null)
            {
                return;
            }
            var data = context.Message;

            await _mediator.Send(new SubmitClassforumAICommand
            {
                UserAIConfig = data.UserAIConfig,
                ClassForumResultId = data.ClassForumResultId,
                SystemRoleAlConfig = data.SystemRoleAlConfig,
                SettingWordMaxLength = data.SettingWordMaxLength,
                SettingTopP = data.SettingTopP,
                SettingTemperature = data.SettingTemperature,
                SettingPresence = data.SettingPresence,
                SettingFrequecy = data.SettingFrequecy,
                SettingModel = data.SettingModel,
                WordContent = data.WordContent,
                IsRetry = data.IsRetry,
            }).ConfigureAwait(false);
        }
    }
}
