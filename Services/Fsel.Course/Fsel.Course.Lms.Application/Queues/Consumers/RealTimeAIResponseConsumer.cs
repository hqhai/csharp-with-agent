// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Consumers
{
    using Fsel.Course.Lms.Application.Commands.AiCmd;
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

        public override async Task ConsumeQueue(ClassForumAIResponseModel? message)
        {
            if (message == null)
            {
                return;
            }

            await _mediator.Send(new SubmitClassforumAICommand
            {
                UserAIConfig = message.UserAIConfig,
                ClassForumResultId = message.ClassForumResultId,
                SystemRoleAlConfig = message.SystemRoleAlConfig,
                SettingWordMaxLength = message.SettingWordMaxLength,
                SettingTopP = message.SettingTopP,
                SettingTemperature = message.SettingTemperature,
                SettingPresence = message.SettingPresence,
                SettingFrequecy = message.SettingFrequecy,
                SettingModel = message.SettingModel,
                WordContent = message.WordContent,
                IsRetry = message.IsRetry,
                DisplayOrder = message.DisplayOrder,
            }).ConfigureAwait(false);
        }
    }
}
