// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Consumers
{
    using Fsel.Course.Lms.Application.Commands.AiCmd;
    using MassTransit;
    using MediatR;
    using Fsel.Course.Domain.Models.QueryModels.ClassForumAutoDot;
    using Fsel.Core.Base;

    public class RealTimeAIResponseConsumer : BaseConsumer<ClassForumAIResponseModel>
    {
        private readonly IMediator _mediator;

        public RealTimeAIResponseConsumer(IMediator mediator, AuthContext authContext) : base(authContext)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(ConsumeContext<Core.Base.BaseModels.BaseQueueDataModel<ClassForumAIResponseModel>> context)
        {
            if (context == null)
            {
                return;
            }
            var data = context.Message?.Data;

            await _mediator.Send(new SubmitClassforumAICommand
            {
                UserAIConfig = data.UserAIConfig,
                ClassForumDetailResultId = data.ClassForumDetailResultId,
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
