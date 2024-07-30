// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Consumers
{
    using Fsel.Core.Base;
    using Fsel.Course.Lms.Application.Commands.AiCmd;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;

    public class RetryMockTestWhenScoreZeroConsumer : BaseConsumer<SetTimeRetryMockTestModel>
    {
        private readonly IMediator _mediator;

        public RetryMockTestWhenScoreZeroConsumer(IMediator mediator, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(SetTimeRetryMockTestModel? message)
        {
            await _mediator.Send(new SubmitMockTestAnswerAICommand
            {
                SectionId = message?.SectionId ?? default,
                SectionGroupId = message?.SectionGroupId ?? default,
                WordContent = message?.WordContent ?? default,
                MockTestResultId = message?.MockTestResultId ?? default,
                IsRetry = message?.IsRetry ?? default,
                SettingModel = message?.SettingModel ?? default,
                SettingTemperature = message?.SettingTemperature ?? default,
                SettingWordMaxLength = message?.SettingWordMaxLength ?? default,
                SettingTopP = message?.SettingTopP ?? default,
                SettingFrequecy = message?.SettingFrequecy ?? default,
                SettingPresence = message?.SettingPresence ?? default,
                SystemRoleAlConfig = message?.SystemRoleAlConfig ?? default,
                UserAIConfig = message?.UserAIConfig ?? default
            }).ConfigureAwait(false);
        }
    }
}
