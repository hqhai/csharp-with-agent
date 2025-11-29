// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Consumers
{
    using Fsel.Course.Lms.Application.Commands.AiCmd;
    using MediatR;
    using Fsel.Course.Domain.Models.QueryModels.ClassForumAutoDot;
    using Fsel.Core.Base;

    public class RealTimeAIResponseConsumer : BaseConsumer<ClassForumAIResponseModelV2>
    {
        private readonly IMediator _mediator;

        public RealTimeAIResponseConsumer(IMediator mediator, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(ClassForumAIResponseModelV2? message)
        {
            if (message == null)
            {
                return;
            }

            await _mediator.Send(new SubmitClassForumAICommand
            {
                //UserAIConfig = message.UserAIConfig,
                ClassForumResultId = message.ClassForumResultId,
                ClassForumDetailResultId = message.ClassForumDetailResultId,
                //SystemRoleAlConfig = message.SystemRoleAlConfig,
                //SettingWordMaxLength = message.SettingWordMaxLength,
                //SettingTopP = message.SettingTopP,
                //SettingTemperature = message.SettingTemperature,
                //SettingPresence = message.SettingPresence,
                //SettingFrequecy = message.SettingFrequecy,
                //SettingModel = message.SettingModel,
                WordContent = message.WordContent,
                IsRetry = message.IsRetry,
                SubmissionCount = message.SubmissionCount,
            }).ConfigureAwait(false);
        }
    }
}
